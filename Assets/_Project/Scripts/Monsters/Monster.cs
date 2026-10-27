using System.Collections;
using System.Collections.Generic;
using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using PuzzleMaster.Items;
using UnityEngine;

namespace PuzzleMaster.Monsters
{
    /// <summary>
    /// A corrupted guardian (ADR-0008, PB-09 / PB-11).
    /// - Out of range: patrols between its waypoints or stands still (<see cref="MonsterMode"/>).
    /// - Player within aggro range: stops, turns to face the player, and its SpellCaster fires.
    /// - Damaged: flashes red (green when it heals). At 0 HP it is **recovered**, not killed:
    ///   it stops moving and casting, turns to the recovered colour, announces
    ///   GameEvents.MonsterRecovered (+100 score, sound) and drops its puzzle item (if any).
    ///   With <see cref="RecoveredStyle.FloatAway"/> (default, ADR-0014) it then holds that pose for a moment,
    ///   rises, spins and fades out (the released spirit), and finally deactivates.
    /// Colours are applied with a MaterialPropertyBlock on the Model's renderers (base colour and, for
    /// glowing materials such as the ghost's, the emission colour), so any model works (theme-agnostic)
    /// and the shared materials are never modified.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Monster : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Behaviour")]
        [Tooltip("Stationary, or Patrol between the waypoints below.")]
        [SerializeField] private MonsterMode mode = MonsterMode.Stationary;
        [Tooltip("Patrol points (scene objects). Positions are read once at start, so they may be anywhere.")]
        [SerializeField] private Transform[] waypoints = new Transform[0];
        [Tooltip("Patrol walking speed in m/s.")]
        [SerializeField] private float patrolSpeed = 2f;
        [Tooltip("Turning speed in degrees per second.")]
        [SerializeField] private float turnSpeed = 360f;
        [Tooltip("Aggro range in metres when no config.json is loaded.")]
        [SerializeField] private float aggroRange = 15f;

        [Header("Parts")]
        [Tooltip("Casts spells at the player while in range.")]
        [SerializeField] private SpellCaster caster;
        [Tooltip("The visual child; its renderers get the hit / heal / recovered colours.")]
        [SerializeField] private Transform model;

        [Header("Recovery")]
        [Tooltip("Puzzle item dropped when recovered. Empty = drops nothing.")]
        [SerializeField, OptionalReference] private ItemDefinition drop;
        [Tooltip("Where the dropped item appears, relative to the monster (local space).")]
        [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0f, 1.5f);
        [Tooltip("Tint once recovered (purified).")]
        [SerializeField] private Color recoveredColor = new Color(1f, 0.95f, 0.75f);
        [Tooltip("Stay: remains in place, purified. FloatAway: after a short hold, rises and fades out (ADR-0014).")]
        [SerializeField] private RecoveredStyle recoveredStyle = RecoveredStyle.FloatAway;
        [Tooltip("FloatAway: seconds the purified pose is held before rising (so it reads as 'recovered', not 'killed').")]
        [SerializeField] private float floatHoldSeconds = 1f;
        [Tooltip("FloatAway: seconds the rise and fade take.")]
        [SerializeField] private float floatDuration = 2f;
        [Tooltip("FloatAway: total rise height in metres.")]
        [SerializeField] private float floatHeight = 3f;
        [Tooltip("FloatAway: spin around the vertical axis (degrees per second).")]
        [SerializeField] private float floatSpin = 120f;
        [Tooltip("FloatAway: transparent URP Lit template (M_GhostFade, made by Setup → Halloween Theme). " +
                 "Referencing it keeps the transparent shader variant in builds. Empty = converted at runtime (may not fade in builds).")]
        [SerializeField, OptionalReference] private Material fadeMaterialTemplate;

        [Header("Feedback")]
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color healColor = new Color(0.3f, 1f, 0.4f);
        [Tooltip("Seconds a hit / heal flash lasts.")]
        [SerializeField] private float flashDuration = 0.15f;
        [Tooltip("Glow strength of a tint on emissive materials (0 = base colour only). No effect on non-glowing materials.")]
        [SerializeField, Range(0f, 2f)] private float tintGlow = 0.6f;

        private Health health;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Vector3[] patrolPoints;
        private int patrolIndex;
        private float flashTimer;
        private bool floatStarted;

        public bool IsRecovered { get; private set; }
        /// <summary>The item this monster drops (for the minimap / HUD later), or null.</summary>
        public ItemDefinition Drop => drop;

        private float AggroRange => ConfigLoader.Current?.monsterAggroRange ?? aggroRange;

        private void Awake()
        {
            health = GetComponent<Health>();
            block = new MaterialPropertyBlock();
            if (model == null)
            {
                Debug.LogWarning($"{name}: Monster.model is empty; using the whole object for colours.", this);
                model = transform;
            }
            renderers = model.GetComponentsInChildren<Renderer>();
            if (caster == null) Debug.LogWarning($"{name}: Monster.caster is empty; this monster can't attack.", this);

            // Read patrol positions once, so waypoints can be children (moving along) or scene objects.
            patrolPoints = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                patrolPoints[i] = waypoints[i] != null ? waypoints[i].position : transform.position;
            if (mode == MonsterMode.Patrol && patrolPoints.Length < 2)
                Debug.LogWarning($"{name}: Patrol needs at least 2 waypoints; standing still instead.", this);
        }

        private void Start()
        {
            if (ConfigLoader.Current != null) health.SetMax(ConfigLoader.Current.monsterMaxHealth, refill: true);
        }

        private void OnEnable()
        {
            health.Changed += OnHealthChanged;
            health.Depleted += Recover;
        }

        private void OnDisable()
        {
            health.Changed -= OnHealthChanged;
            health.Depleted -= Recover;
        }

        private void Update()
        {
            UpdateFlash();
            if (IsRecovered) return;

            Health player = Health.PlayerInstance;
            bool inRange = player != null
                           && (player.transform.position - transform.position).sqrMagnitude <= AggroRange * AggroRange;
            if (inRange)
            {
                TurnTowards(player.transform.position);
                if (caster != null) caster.TryCast(player.transform);
            }
            else if (mode == MonsterMode.Patrol && patrolPoints.Length >= 2)
            {
                Patrol();
            }
        }

        private void Patrol()
        {
            Vector3 goal = patrolPoints[patrolIndex];
            goal.y = transform.position.y;   // walk on the ground plane
            transform.position = Vector3.MoveTowards(transform.position, goal, patrolSpeed * Time.deltaTime);
            TurnTowards(goal);
            if ((goal - transform.position).sqrMagnitude < 0.04f)
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        /// <summary>Turns around the vertical axis only, at most turnSpeed degrees per second.</summary>
        private void TurnTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            Quaternion goal = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
        }

        private void OnHealthChanged(float delta)
        {
            if (IsRecovered || delta == 0f) return;
            Tint(delta < 0f ? hitColor : healColor);
            flashTimer = flashDuration;
            if (delta > 0f) GameEvents.RaiseMonsterHealed(transform.position);
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f) return;
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && !IsRecovered) ClearTint();
        }

        /// <summary>0 HP: purified instead of killed. Stops, changes colour, scores, drops its item.</summary>
        private void Recover()
        {
            if (IsRecovered) return;
            IsRecovered = true;
            if (caster != null) caster.enabled = false;
            Tint(recoveredColor);
            Debug.Log($"{name} recovered" + (drop != null ? $", dropped {drop.DisplayName}" : ""), this);
            GameEvents.RaiseMonsterRecovered(transform.position);

            if (drop == null) return;
            if (drop.PickupPrefab == null)
            {
                Debug.LogError($"{drop.name} has no pickup prefab; nothing dropped.", drop);
                return;
            }
            Instantiate(drop.PickupPrefab, transform.TransformPoint(dropOffset), Quaternion.identity);
        }

        // Recover() returns early when there is no drop, so the float-away starts from its own callback.
        private void LateUpdate()
        {
            if (IsRecovered && recoveredStyle == RecoveredStyle.FloatAway && !floatStarted)
            {
                floatStarted = true;
                StartCoroutine(FloatAway());
            }
        }

        /// <summary>
        /// The released spirit: hold the purified pose, then rise with a spin while the glow and the
        /// colour fade to nothing, then deactivate. Uses per-monster transparent material copies
        /// (MaterialUtility, ported from PA1's ghost fade) so other ghosts are unaffected.
        /// </summary>
        private IEnumerator FloatAway()
        {
            yield return new WaitForSeconds(floatHoldSeconds);

            // Nothing to bump into or target while it rises.
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            List<Material> fades = PrepareFadeMaterials();
            Vector3 start = transform.position;

            for (float t = 0f; t < floatDuration; t += Time.deltaTime)
            {
                float k = t / floatDuration;
                // SmoothStep eases the rise in and out; alpha follows the same curve down.
                float eased = Mathf.SmoothStep(0f, 1f, k);
                transform.position = start + Vector3.up * (floatHeight * eased);
                transform.Rotate(0f, floatSpin * Time.deltaTime, 0f, Space.World);
                float alpha = 1f - eased;
                foreach (Material m in fades)
                {
                    m.SetColor(MaterialUtility.BaseColor, new Color(recoveredColor.r, recoveredColor.g, recoveredColor.b, alpha));
                    m.SetColor(MaterialUtility.EmissionColor, recoveredColor * (tintGlow * alpha));
                }
                yield return null;
            }
            foreach (Material m in fades) Destroy(m);   // runtime copies: free them
            gameObject.SetActive(false);
        }

        /// <summary>Swaps every model renderer to a transparent copy (template + the original textures).</summary>
        private List<Material> PrepareFadeMaterials()
        {
            if (fadeMaterialTemplate == null)
                Debug.LogWarning($"{name}: no Fade Material Template; converting materials at runtime (may not fade in a build).", this);
            var fades = new List<Material>();
            foreach (Renderer r in renderers)
            {
                r.SetPropertyBlock(null);   // the tint block would override the fading colour
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // a fading ghost casts no shadow
                Material[] materials = r.materials;   // per-renderer instances
                for (int i = 0; i < materials.Length; i++)
                {
                    Material fade;
                    if (fadeMaterialTemplate != null)
                    {
                        fade = new Material(fadeMaterialTemplate);
                        MaterialUtility.CopyLitProperties(materials[i], fade);
                    }
                    else
                    {
                        fade = new Material(materials[i]);
                        MaterialUtility.MakeTransparent(fade);
                    }
                    Destroy(materials[i]);   // the instance created by r.materials
                    materials[i] = fade;
                    fades.Add(fade);
                }
                r.materials = materials;
            }
            return fades;
        }

        private void Tint(Color color)
        {
            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(EmissionColorId, color * tintGlow);   // ignored unless the material glows
                r.SetPropertyBlock(block);
            }
        }

        private void ClearTint()
        {
            foreach (Renderer r in renderers) r.SetPropertyBlock(null);   // back to the material's own colour
        }

        // Scene view helper: aggro range (red) and patrol route (yellow) when the monster is selected.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, AggroRange);
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
            }
        }
    }
}
