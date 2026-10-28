using System;
using UnityEngine;

namespace PuzzleMaster.Combat
{
    /// <summary>
    /// Hit points for the player and the monsters (ADR-0008). Damage and heal go through here,
    /// and other components react to the events: the life bar redraws on <see cref="Changed"/>,
    /// a monster is recovered on <see cref="Depleted"/>.
    /// Once depleted it ignores further damage and healing (a recovered monster stays recovered).
    /// The player's Health also registers itself as <see cref="PlayerInstance"/>, so prefabs
    /// (monsters, spells) can find their target without a scene reference.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Tooltip("Side: Player attacks damage Monster, monster spells damage Player.")]
        [SerializeField] private Team team = Team.Monster;
        [Tooltip("Maximum HP. Owners (Monster, PlayerHitResponse) may replace it with the config.json value.")]
        [SerializeField] private float maxHealth = 100f;

        /// <summary>Raised on every change with the signed amount (negative = damage, positive = heal).</summary>
        public event Action<float> Changed;
        /// <summary>Raised once when HP reaches 0.</summary>
        public event Action Depleted;

        /// <summary>The player's Health in the loaded scene, or null.</summary>
        public static Health PlayerInstance { get; private set; }

        public Team Team => team;
        public float Max => maxHealth;
        public float Current { get; private set; }
        /// <summary>Current / Max, 0…1 (for life bars).</summary>
        public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;
        public bool IsDepleted => Current <= 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => PlayerInstance = null;

        private void Awake() => Current = maxHealth;

        private void OnEnable()
        {
            if (team == Team.Player) PlayerInstance = this;
        }

        private void OnDisable()
        {
            if (PlayerInstance == this) PlayerInstance = null;
        }

        /// <summary>Sets a new maximum; with <paramref name="refill"/> HP becomes full.</summary>
        public void SetMax(float max, bool refill)
        {
            maxHealth = Mathf.Max(1f, max);
            Current = refill ? maxHealth : Mathf.Min(Current, maxHealth);
            Changed?.Invoke(0f);
        }

        // RUBRIC 1b · Monster attack / recovery (10): "lose health when attacked"; Depleted fires once at 0 HP.
        /// <summary>Removes HP; returns true if damage was applied (false when already depleted).</summary>
        public bool Damage(float amount)
        {
            if (IsDepleted || amount <= 0f) return false;
            float before = Current;
            Current = Mathf.Max(0f, Current - amount);
            Changed?.Invoke(Current - before);
            if (IsDepleted) Depleted?.Invoke();
            return true;
        }

        /// <summary>Adds HP up to the maximum; returns the amount actually healed.</summary>
        public float Heal(float amount)
        {
            if (IsDepleted || amount <= 0f || Current >= maxHealth) return 0f;
            float before = Current;
            Current = Mathf.Min(maxHealth, Current + amount);
            Changed?.Invoke(Current - before);
            return Current - before;
        }

        /// <summary>Back to full HP, even from 0 (used by the respawn demo toggle).</summary>
        public void Refill()
        {
            Current = maxHealth;
            Changed?.Invoke(0f);
        }
    }
}
