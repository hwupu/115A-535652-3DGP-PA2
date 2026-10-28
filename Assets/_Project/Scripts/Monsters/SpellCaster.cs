using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Monsters
{
    /// <summary>
    /// Fires spell projectiles at a target, at most once per interval (PB-10).
    /// The Monster decides when (player in range) and calls <see cref="TryCast"/> every frame;
    /// this component only handles the timing and the spawning.
    /// </summary>
    public class SpellCaster : MonoBehaviour
    {
        [Tooltip("The spell prefab (SpellProjectile).")]
        [SerializeField] private SpellProjectile spellPrefab;
        [Tooltip("Where spells appear (e.g. the hands). Empty = 1.4 m above the monster's pivot.")]
        [SerializeField, OptionalReference] private Transform castPoint;

        [Header("Fallbacks when no config.json is loaded")]
        [Tooltip("Seconds between two spells.")]
        [SerializeField] private float interval = 2.5f;
        [Tooltip("Spell flight speed in m/s.")]
        [SerializeField] private float speed = 8f;
        [Tooltip("Damage per spell hit (the player's HP has no gameplay effect, ADR-0008).")]
        [SerializeField] private float damage = 10f;

        [Tooltip("Height above the target's feet to aim at (chest).")]
        [SerializeField] private float aimHeight = 1f;

        private const float DefaultCastHeight = 1.4f;   // used when Cast Point is empty (roughly the hands)
        private const float FirstDelayMin = 0.5f, FirstDelayMax = 1.2f;

        private float nextCastTime;

        private float Interval => ConfigLoader.Current?.spellInterval ?? interval;
        private float Speed => ConfigLoader.Current?.spellSpeed ?? speed;
        private float Damage => ConfigLoader.Current?.spellDamage ?? damage;

        // A short random first delay, so monsters that see the player together don't fire in sync.
        private void OnEnable() => nextCastTime = Time.time + Random.Range(FirstDelayMin, FirstDelayMax);

        // RUBRIC 1b · Monster attack / recovery (10): monsters cast spell projectiles toward the player.
        /// <summary>Casts at <paramref name="target"/> if the interval has passed. Returns true if a spell was fired.</summary>
        public bool TryCast(Transform target)
        {
            if (!enabled || target == null || Time.time < nextCastTime) return false;
            if (spellPrefab == null)
            {
                Debug.LogError($"{name}: SpellCaster has no spell prefab.", this);
                enabled = false;
                return false;
            }
            nextCastTime = Time.time + Interval;

            Vector3 origin = castPoint != null ? castPoint.position : transform.position + Vector3.up * DefaultCastHeight;
            Vector3 aim = target.position + Vector3.up * aimHeight;
            SpellProjectile spell = Instantiate(spellPrefab, origin, Quaternion.identity);
            spell.Launch(aim - origin, Speed, Damage, Team.Monster, transform);
            GameEvents.RaiseSpellCast(origin);
            return true;
        }
    }
}
