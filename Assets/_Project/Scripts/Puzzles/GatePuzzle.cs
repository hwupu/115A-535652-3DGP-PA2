using System.Collections;
using System.Text;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// The final gate (PB-22). Sealed (locked colour) until every prerequisite puzzle (1–4) is solved;
    /// then its bars slide away. Entering the open gate's trigger zone solves it, which is the last
    /// puzzle, so the PuzzleManager declares the win. Entering while sealed lists what is missing.
    /// Needs a trigger collider and a kinematic Rigidbody on this object (set up by Setup → Sprint 3).
    /// </summary>
    public class GatePuzzle : Puzzle
    {
        [Tooltip("Puzzles that must be solved before the gate opens.")]
        [SerializeField] private Puzzle[] prerequisites = new Puzzle[0];

        private bool unlocked;

        protected override bool IsLocked => !unlocked;

        public override string Status =>
            IsSolved ? "solved"
            : unlocked ? "open: walk through"
            : $"sealed: {SolvedPrerequisites}/{prerequisites.Length} puzzles";

        private int SolvedPrerequisites
        {
            get
            {
                int n = 0;
                foreach (Puzzle p in prerequisites)
                    if (p != null && p.IsSolved) n++;
                return n;
            }
        }

        private void OnEnable()
        {
            foreach (Puzzle p in prerequisites)
                if (p != null) p.Solved += OnPrerequisiteSolved;
        }

        private void OnDisable()
        {
            foreach (Puzzle p in prerequisites)
                if (p != null) p.Solved -= OnPrerequisiteSolved;
        }

        protected override void Start()
        {
            base.Start();
            TryUnlock();   // no prerequisites (or all solved via debug) = open from the start
        }

        private void OnPrerequisiteSolved(Puzzle _)
        {
            NotifyChanged();   // "sealed: n/4" changed
            // One frame later, so "… solved! +200" is announced before "The gate is open!".
            StartCoroutine(UnlockNextFrame());
        }

        private IEnumerator UnlockNextFrame()
        {
            yield return null;
            TryUnlock();
        }

        private void TryUnlock()
        {
            if (unlocked || SolvedPrerequisites < prerequisites.Length) return;
            unlocked = true;
            Open();
            GameEvents.RaisePuzzleProgressed(transform.position);
            GameEvents.RaisePuzzleMessage($"The {Title} is open!");
            NotifyChanged();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (unlocked || IsSolved || PlayerInventory(other) == null) return;
            GameEvents.RaisePuzzleMessage($"The {Title} is sealed. Still to solve: {MissingList()}.");
        }

        // Stay (not only Enter): a player already standing in the zone when the gate opens also counts.
        private void OnTriggerStay(Collider other)
        {
            if (unlocked && !IsSolved && PlayerInventory(other) != null) MarkSolved();
        }

        private string MissingList()
        {
            var sb = new StringBuilder();
            foreach (Puzzle p in prerequisites)
            {
                if (p == null || p.IsSolved) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(p.Title);
            }
            return sb.ToString();
        }
    }
}
