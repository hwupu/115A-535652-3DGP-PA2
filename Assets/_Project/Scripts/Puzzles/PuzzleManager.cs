using System;
using System.Collections.Generic;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// Completion logic (PB-17, ADR-0009): counts solved puzzles, awards the solve score
    /// (+200 by default, config.json), and when all are solved tells the GameStateManager to Win.
    /// Each solve message also restores one of the story's five Crystal Seals (PB-27).
    /// The HUD objective list listens to <see cref="Changed"/>.
    /// The puzzle list is linked by Setup → Sprint 3 (the five sites in spec order).
    /// </summary>
    public class PuzzleManager : MonoBehaviour
    {
        [Tooltip("All puzzles of the level, in the order the HUD lists them.")]
        [SerializeField] private Puzzle[] puzzles = new Puzzle[0];
        [Tooltip("Told to Win when every puzzle is solved.")]
        [SerializeField] private GameStateManager gameState;
        [Tooltip("Receives the solve points.")]
        [SerializeField] private ScoreManager score;
        [Tooltip("Story line added to every solve message (PB-27, the spec's Hero's Journey). Empty = none.")]
        [SerializeField] private string solvedStoryLine = "A Crystal Seal is restored.";

        /// <summary>Raised when any puzzle's status changes (progress or solve).</summary>
        public event Action Changed;

        public IReadOnlyList<Puzzle> Puzzles => puzzles;
        public int Total => puzzles.Length;
        public int SolvedCount
        {
            get
            {
                int n = 0;
                foreach (Puzzle p in puzzles)
                    if (p != null && p.IsSolved) n++;
                return n;
            }
        }

        private void OnEnable()
        {
            if (puzzles.Length == 0) Debug.LogError($"{nameof(PuzzleManager)}: no puzzles assigned; the game can't be won. Run Setup → Sprint 3.", this);
            if (gameState == null) Debug.LogError($"{nameof(PuzzleManager)}: Game State is not assigned; solving everything won't win.", this);
            if (score == null) Debug.LogError($"{nameof(PuzzleManager)}: Score is not assigned; solves give no points.", this);
            foreach (Puzzle p in puzzles)
            {
                if (p == null) continue;
                p.Solved += OnPuzzleSolved;
                p.Changed += OnPuzzleChanged;
            }
        }

        private void OnDisable()
        {
            foreach (Puzzle p in puzzles)
            {
                if (p == null) continue;
                p.Solved -= OnPuzzleSolved;
                p.Changed -= OnPuzzleChanged;
            }
        }

        private void OnPuzzleSolved(Puzzle puzzle)
        {
            int points = score != null ? score.SolvePuzzlePoints : 0;
            if (score != null) score.Add(points, $"puzzle solved: {puzzle.Title}");
            string story = string.IsNullOrEmpty(solvedStoryLine) ? "" : " " + solvedStoryLine;
            GameEvents.RaisePuzzleMessage($"{puzzle.Title} solved!{story} +{points}   ({SolvedCount}/{Total})");

            if (SolvedCount < Total) return;
            Debug.Log("All puzzles solved → Win");
            if (gameState != null) gameState.Win();
        }

        private void OnPuzzleChanged(Puzzle _) => Changed?.Invoke();

        // Inspector ⋮ menu (Play Mode): jump to the final gate while testing.
        [ContextMenu("Debug: Solve all except the last")]
        private void DebugSolveAllButLast()
        {
            if (!Application.isPlaying) return;
            for (int i = 0; i < puzzles.Length - 1; i++)
                if (puzzles[i] != null) puzzles[i].DebugSolve();
        }
    }
}
