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
    /// Colours are applied with a MaterialPropertyBlock on the Model's renderers, so any model works
    /// (theme-agnostic) and the shared materials are never modified.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Monster : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

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

        [Header("Feedback")]
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color healColor = new Color(0.3f, 1f, 0.4f);
        [Tooltip("Seconds a hit / heal flash lasts.")]
        [SerializeField] private float flashDuration = 0.15f;

        private Health health;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Vector3[] patrolPoints;
        private int patrolIndex;
        private float flashTimer;

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

        private void Tint(Color color)
        {
            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
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
