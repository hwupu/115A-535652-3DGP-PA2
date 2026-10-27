using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Player
{
    /// <summary>
    /// What happens when a spell hits the player (PB-10, ADR-0008).
    /// PO decision: the player's HP is tracked **in the background** only. Spells hit and HP
    /// drops (logged, GameEvents.PlayerHit for sound), but nothing else happens, matching the
    /// spec where the only way to lose is the timer.
    /// The respawn at 0 HP exists for the TA demo and is switched by the DEMO TOGGLE in OnEnable.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerHitResponse : MonoBehaviour
    {
        [Tooltip("Moves the player back to the start on respawn.")]
        [SerializeField] private PlayerMotor motor;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (motor == null) Debug.LogWarning($"{nameof(PlayerHitResponse)}: Motor is empty; respawn (demo toggle) can't move the player.", this);
        }

        private void Start()
        {
            if (ConfigLoader.Current != null) health.SetMax(ConfigLoader.Current.playerMaxHealth, refill: true);
        }

        private void OnEnable()
        {
            health.Changed += OnHealthChanged;

            // DEMO TOGGLE: player at 0 HP (ADR-0008).
            //   Commented (default, matches the spec): HP reaches 0 and nothing happens.
            //   Uncommented: the player respawns at the start with full HP (not in the spec).
            // health.Depleted += RespawnAtStart;
        }

        private void OnDisable()
        {
            health.Changed -= OnHealthChanged;
            health.Depleted -= RespawnAtStart;   // removing a handler that was never added is harmless
        }

        private void OnHealthChanged(float delta)
        {
            if (delta >= 0f) return;
            Debug.Log($"Player hit by a spell: HP {health.Current:0}/{health.Max:0} (background only, no gameplay effect)", this);
            GameEvents.RaisePlayerHit(health.Current, health.Max);
        }

        /// <summary>Demo-toggle behaviour: back to the start position with full HP.</summary>
        private void RespawnAtStart()
        {
            Debug.Log("Player HP reached 0: respawning at the start (demo toggle).", this);
            if (motor != null) motor.ResetToStart();
            health.Refill();
        }
    }
}
