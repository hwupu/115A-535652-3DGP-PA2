using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Monsters
{
    /// <summary>
    /// Monsters cure themselves (PO decision, PB-13, ADR-0008). Two modes, switched by a
    /// DEMO TOGGLE in <see cref="Update"/> (exactly one line active):
    /// - default: heal <c>monsterHealAmount</c> every <c>monsterHealInterval</c> (10 s) while damaged;
    /// - alternative: heal once, the first time HP drops below 50 %.
    /// A recovered monster (0 HP) never heals.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class MonsterSelfHeal : MonoBehaviour
    {
        [Header("Fallbacks when no config.json is loaded")]
        [Tooltip("Seconds between heals (default mode).")]
        [SerializeField] private float interval = 10f;
        [Tooltip("HP restored per heal.")]
        [SerializeField] private float amount = 20f;

        [Tooltip("HP fraction that triggers the one-time heal (alternative mode).")]
        [SerializeField, Range(0.05f, 0.95f)] private float onceThreshold = 0.5f;

        private Health health;
        private float timer;
        private bool usedOnce;

        private float Interval => ConfigLoader.Current?.monsterHealInterval ?? interval;
        private float Amount => ConfigLoader.Current?.monsterHealAmount ?? amount;

        private void Awake() => health = GetComponent<Health>();

        private void Update()
        {
            // Nothing to heal at full HP, and a recovered monster stays recovered.
            if (health.IsDepleted || health.Current >= health.Max)
            {
                timer = 0f;
                return;
            }

            // DEMO TOGGLE: monster self-heal mode (ADR-0008). Keep exactly ONE of the two lines active.
            //   HealEveryInterval  = default: heal every monsterHealInterval seconds (10 s) while damaged.
            //   HealOnceBelowHalf  = alternative: heal once, the first time HP drops below 50 %.
            HealEveryInterval();
            // HealOnceBelowHalf();
        }

        /// <summary>Default mode: a heal each time the damaged monster has waited the full interval.</summary>
        private void HealEveryInterval()
        {
            timer += Time.deltaTime;
            if (timer < Interval) return;
            timer = 0f;
            Heal();
        }

        /// <summary>Alternative mode: a single heal when HP first falls below the threshold.</summary>
        private void HealOnceBelowHalf()
        {
            if (usedOnce || health.Normalized >= onceThreshold) return;
            usedOnce = true;
            Heal();
        }

        private void Heal()
        {
            float healed = health.Heal(Amount);
            if (healed > 0f) Debug.Log($"{name} healed itself +{healed:0} HP ({health.Current:0}/{health.Max:0})", this);
        }
    }
}
