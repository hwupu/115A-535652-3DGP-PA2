using System.Collections;
using System.Collections.Generic;
using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Player
{
    /// <summary>
    /// Melee attack on LMB (ADR-0008, PB-11). Each swing hits every monster Health inside a
    /// short cone in front of the player: OverlapSphere(range) finds candidates, then an angle
    /// check keeps only those in front. A cooldown limits the swing rate.
    /// Hits raise GameEvents.MonsterHit (the hit sound, PB-16); every swing raises AttackSwung.
    /// An optional pivot (e.g. holding a sword) sweeps left → right as visual feedback.
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [Tooltip("Input source (Attack = LMB).")]
        [SerializeField] private GameInput input;
        [Tooltip("Optional visual that sweeps during a swing (e.g. Model/SwordPivot).")]
        [SerializeField, OptionalReference] private Transform swingPivot;

        [Header("Fallbacks when no config.json is loaded")]
        [Tooltip("Damage per hit (a 100 HP monster needs 4 hits).")]
        [SerializeField] private float damage = 25f;
        [Tooltip("Reach of a swing in metres.")]
        [SerializeField] private float range = 2.5f;
        [Tooltip("Minimum seconds between two swings.")]
        [SerializeField] private float cooldown = 0.5f;

        [Header("Shape")]
        [Tooltip("Width of the hit cone in degrees (centred on the player's facing).")]
        [SerializeField, Range(10f, 360f)] private float coneAngle = 120f;
        [Tooltip("Height of the attack centre above the feet.")]
        [SerializeField] private float attackHeight = 1f;
        [Tooltip("Seconds the visual sweep takes.")]
        [SerializeField] private float swingDuration = 0.15f;

        /// <summary>Raised on every swing that passes the cooldown (hit or miss), e.g. for the swing animation.</summary>
        public event System.Action Swung;

        private const float TouchingDistanceSqr = 0.5f * 0.5f;   // closer than 0.5 m always hits, whatever the angle
        private const float SweepAngle = 60f;                     // visual sweep goes from −60° to +60°

        private readonly Collider[] overlaps = new Collider[32];   // reused every swing (NonAlloc: no garbage)
        private readonly HashSet<Health> hitThisSwing = new HashSet<Health>();
        private float nextSwingTime;

        private float Damage => ConfigLoader.Current?.attackDamage ?? damage;
        private float Range => ConfigLoader.Current?.attackRange ?? range;
        private float Cooldown => ConfigLoader.Current?.attackCooldown ?? cooldown;

        private void OnEnable()
        {
            if (input != null) input.AttackPressed += Swing;
            else Debug.LogError($"{nameof(PlayerAttack)}: Input is not assigned; the player can't attack.", this);
        }

        private void OnDisable()
        {
            if (input != null) input.AttackPressed -= Swing;
        }

        // RUBRIC 1b · Monster attack / recovery (10): the player's attack. LMB → OverlapSphere → cone filter → Health.Damage.
        private void Swing()
        {
            if (Time.time < nextSwingTime) return;
            nextSwingTime = Time.time + Cooldown;

            Vector3 center = transform.position + Vector3.up * attackHeight;
            int count = Physics.OverlapSphereNonAlloc(center, Range, overlaps, ~0, QueryTriggerInteraction.Ignore);
            hitThisSwing.Clear();   // a monster with several colliders is hit once per swing
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                // The collider may sit on a child; Health is on the monster root. Identified by component, not tag.
                var target = overlaps[i].GetComponentInParent<Health>();
                if (target == null || target.Team != Team.Monster || target.IsDepleted || !hitThisSwing.Add(target)) continue;
                if (!InCone(target.transform.position)) continue;

                target.Damage(Damage);
                GameEvents.RaiseMonsterHit(target.transform.position);
                hits++;
            }

            Swung?.Invoke();
            GameEvents.RaiseAttackSwung(transform.position);
            if (swingPivot != null) StartCoroutine(SweepVisual());
            if (hits == 0) Debug.Log("Swing: no monster in reach");
        }

        /// <summary>True if the point is within the cone in front of the player (flat angle check).</summary>
        private bool InCone(Vector3 point)
        {
            Vector3 toTarget = point - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < TouchingDistanceSqr) return true;
            // Angle is 0…180° either side of forward, so half the cone width on each side.
            return Vector3.Angle(transform.forward, toTarget) <= coneAngle * 0.5f;
        }

        /// <summary>Sweeps the pivot from −60° to +60° around the vertical axis, then back to rest.</summary>
        private IEnumerator SweepVisual()
        {
            for (float t = 0f; t < swingDuration; t += Time.deltaTime)
            {
                swingPivot.localRotation = Quaternion.Euler(0f, Mathf.Lerp(-SweepAngle, SweepAngle, t / swingDuration), 0f);
                yield return null;
            }
            swingPivot.localRotation = Quaternion.identity;
        }

        // Scene view helper: the attack reach when the player is selected.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * attackHeight, Range);
        }
    }
}
