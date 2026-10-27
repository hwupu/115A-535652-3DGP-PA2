using System;
using System.Text;
using UnityEngine;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Runtime-tunable settings read from config.json (ADR-0004, PB-08).
    /// Field names are the JSON keys, so they stay lowerCamelCase.
    /// A plain [Serializable] C# class (not a MonoBehaviour), because JsonUtility only serializes
    /// public / [SerializeField] fields. Public fields are an exception to our "no public fields"
    /// rule here: they are the JSON schema. ConfigLoader creates and validates instances.
    /// Defaults follow the spec (5-minute timer, +100 recover, +200 puzzle); the rest are
    /// starting values to tune in play-tests.
    /// </summary>
    [Serializable]
    public class GameConfig
    {
        [Header("Game flow")]
        [Tooltip("Countdown length in seconds. Spec: 5 minutes = 300. Shorten it to demo the lose screen.")]
        public float timeLimitSeconds = 300f;

        [Header("Score (spec examples)")]
        [Tooltip("Points for recovering (purifying) a monster.")]
        public int recoverMonsterScore = 100;
        [Tooltip("Points for solving a puzzle.")]
        public int solvePuzzleScore = 200;

        [Header("Player")]
        [Tooltip("Walking speed in m/s.")]
        public float moveSpeed = 5f;
        [Tooltip("Mouse look sensitivity (degrees per pixel of mouse movement).")]
        public float mouseSensitivity = 0.15f;
        [Tooltip("Player HP. Tracked in the background only; spells have no gameplay effect (ADR-0008).")]
        public float playerMaxHealth = 100f;

        [Header("Player attack (melee)")]
        [Tooltip("Damage per hit on a monster.")]
        public float attackDamage = 25f;
        [Tooltip("Reach of the melee swing in metres.")]
        public float attackRange = 2.5f;
        [Tooltip("Seconds between swings.")]
        public float attackCooldown = 0.5f;

        [Header("Monsters")]
        [Tooltip("Monster HP.")]
        public float monsterMaxHealth = 100f;
        [Tooltip("Distance at which a monster notices the player and starts casting.")]
        public float monsterAggroRange = 15f;
        [Tooltip("Seconds between spells.")]
        public float spellInterval = 2.5f;
        [Tooltip("Spell projectile speed in m/s.")]
        public float spellSpeed = 8f;
        [Tooltip("Damage a spell deals to the background player HP.")]
        public float spellDamage = 10f;
        [Tooltip("Seconds between self-heals (default mode, ADR-0008).")]
        public float monsterHealInterval = 10f;
        [Tooltip("HP restored per self-heal.")]
        public float monsterHealAmount = 20f;

        /// <summary>
        /// Clamps values to sane gameplay limits so a typo in the file can't break the demo.
        /// Returns a description of every value that was changed (empty if none).
        /// </summary>
        public string Validate()
        {
            var changes = new StringBuilder();
            // Local function: clamps one value and records "key: old → new" when it changed.
            float Clamp(string key, float value, float min, float max)
            {
                float clamped = Mathf.Clamp(value, min, max);
                if (!Mathf.Approximately(value, clamped)) changes.Append($"{key}: {value} → {clamped}; ");
                return clamped;
            }
            int ClampInt(string key, int value, int min, int max) => Mathf.RoundToInt(Clamp(key, value, min, max));

            timeLimitSeconds = Clamp(nameof(timeLimitSeconds), timeLimitSeconds, 10f, 3600f);
            recoverMonsterScore = ClampInt(nameof(recoverMonsterScore), recoverMonsterScore, 0, 10000);
            solvePuzzleScore = ClampInt(nameof(solvePuzzleScore), solvePuzzleScore, 0, 10000);

            moveSpeed = Clamp(nameof(moveSpeed), moveSpeed, 1f, 20f);
            mouseSensitivity = Clamp(nameof(mouseSensitivity), mouseSensitivity, 0.01f, 2f);
            playerMaxHealth = Clamp(nameof(playerMaxHealth), playerMaxHealth, 1f, 10000f);

            attackDamage = Clamp(nameof(attackDamage), attackDamage, 1f, 1000f);
            attackRange = Clamp(nameof(attackRange), attackRange, 0.5f, 10f);
            attackCooldown = Clamp(nameof(attackCooldown), attackCooldown, 0.05f, 5f);

            monsterMaxHealth = Clamp(nameof(monsterMaxHealth), monsterMaxHealth, 1f, 10000f);
            monsterAggroRange = Clamp(nameof(monsterAggroRange), monsterAggroRange, 1f, 100f);
            spellInterval = Clamp(nameof(spellInterval), spellInterval, 0.2f, 30f);
            spellSpeed = Clamp(nameof(spellSpeed), spellSpeed, 1f, 50f);
            spellDamage = Clamp(nameof(spellDamage), spellDamage, 0f, 1000f);
            monsterHealInterval = Clamp(nameof(monsterHealInterval), monsterHealInterval, 1f, 600f);
            monsterHealAmount = Clamp(nameof(monsterHealAmount), monsterHealAmount, 0f, 10000f);

            return changes.ToString();
        }
    }
}
