using System;
using UnityEngine;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Holds the score (PB-15). Listens to GameEvents.MonsterRecovered (+100 by default);
    /// puzzles add +200 through <see cref="Add"/> in Sprint 3. Points come from config.json
    /// (recoverMonsterScore, solvePuzzleScore) or the Inspector fallbacks.
    /// The HUD and the end screens listen to <see cref="Changed"/> or read <see cref="Score"/>.
    /// The score resets with the scene on restart.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Fallbacks when no config.json is loaded (spec examples)")]
        [SerializeField] private int recoverMonsterScore = 100;
        [SerializeField] private int solvePuzzleScore = 200;

        /// <summary>Raised with the new total whenever points are added.</summary>
        public event Action<int> Changed;

        public int Score { get; private set; }

        /// <summary>Points for solving one puzzle (used by the puzzle system).</summary>
        public int SolvePuzzlePoints => ConfigLoader.Current?.solvePuzzleScore ?? solvePuzzleScore;
        private int RecoverMonsterPoints => ConfigLoader.Current?.recoverMonsterScore ?? recoverMonsterScore;

        private void OnEnable() => GameEvents.MonsterRecovered += OnMonsterRecovered;
        private void OnDisable() => GameEvents.MonsterRecovered -= OnMonsterRecovered;

        private void OnMonsterRecovered(Vector3 _) => Add(RecoverMonsterPoints, "monster recovered");

        /// <summary>Adds points (ignored if not positive) and announces the new total.</summary>
        public void Add(int points, string reason)
        {
            if (points <= 0) return;
            Score += points;
            Debug.Log($"Score +{points} ({reason}) = {Score}");
            Changed?.Invoke(Score);
        }
    }
}
