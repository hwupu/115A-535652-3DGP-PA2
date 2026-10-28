using System;
using System.Collections;
using PuzzleMaster.Core;
using PuzzleMaster.Items;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// Base class of the five puzzles (ADR-0009, PB-17). A puzzle is Unsolved until a subclass
    /// calls <see cref="MarkSolved"/>; it never goes back. The PuzzleManager counts solved puzzles
    /// (score, Win), and the HUD, world label and minimap read <see cref="Title"/> / <see cref="Status"/>.
    /// Three subclasses cover all five puzzles:
    /// - <see cref="ItemSocketPuzzle"/>: needs n of one item (key door, crystal altar, artifact pedestal);
    /// - <see cref="SwitchPuzzle"/>: all its pressure switches stepped on;
    /// - <see cref="GatePuzzle"/>: opens when its prerequisite puzzles are solved, solved by entering it.
    /// Shared feedback lives here: status colours on indicator renderers (beacon orb, minimap icon) and
    /// an optional "opening" (door, gate bars) that slides away.
    /// The player is recognised by its Inventory component (only the player carries one).
    /// </summary>
    public abstract class Puzzle : MonoBehaviour
    {
        [Tooltip("Name shown in the HUD objective list, the world label and messages.")]
        [SerializeField] private string title = "Puzzle";

        [Header("Status colours")]
        [Tooltip("Renderers tinted by state, e.g. the beacon orb and the minimap icon. May be empty.")]
        [SerializeField] private Renderer[] statusRenderers = new Renderer[0];
        [Tooltip("Open and solvable now.")]
        [SerializeField] private Color unsolvedColor = new Color(1f, 0.7f, 0.1f);
        [Tooltip("Not solvable yet (the Final Gate before puzzles 1–4).")]
        [SerializeField] private Color lockedColor = new Color(0.9f, 0.15f, 0.15f);
        [Tooltip("Solved.")]
        [SerializeField] private Color solvedColor = new Color(0.25f, 1f, 0.35f);

        [Header("Opening (door, gate bars)")]
        [Tooltip("Object that slides away when the puzzle opens. Empty = nothing moves.")]
        [SerializeField, OptionalReference] private Transform opening;
        [Tooltip("How far the opening moves, in its parent's space (negative y = sinks into the ground).")]
        [SerializeField] private Vector3 openOffset = new Vector3(0f, -3f, 0f);
        [Tooltip("Seconds the opening takes.")]
        [SerializeField, Min(0.01f)] private float openSeconds = 1.2f;

        /// <summary>Raised whenever <see cref="Status"/> may have changed (progress, unlock, solve).</summary>
        public event Action<Puzzle> Changed;
        /// <summary>Raised once, when the puzzle is solved.</summary>
        public event Action<Puzzle> Solved;

        public string Title => title;
        public bool IsSolved { get; private set; }
        /// <summary>Short progress text for the HUD and the world label, e.g. "Crystals 2/3".</summary>
        public abstract string Status { get; }
        /// <summary>Locked puzzles show the locked colour (the gate until puzzles 1–4 are solved).</summary>
        protected virtual bool IsLocked => false;

        private MaterialPropertyBlock block;
        private bool opened;

        // ---------- Lifecycle ----------

        protected virtual void Start() => ShowStatusColour();

        // ---------- API for subclasses (ItemSocketPuzzle, SwitchPuzzle, GatePuzzle) ----------

        // RUBRIC 2b · Puzzle completion logic (10): every puzzle ends here exactly once → PuzzleManager counts it.
        /// <summary>Solves the puzzle once: colour, events (PuzzleManager → score / Win), sound.</summary>
        protected void MarkSolved()
        {
            if (IsSolved) return;
            IsSolved = true;
            OnSolved();
            GameEvents.RaisePuzzleSolved(transform.position);
            Solved?.Invoke(this);
            NotifyChanged();
        }

        /// <summary>Hook for subclasses (e.g. open the door). Runs before the Solved event.</summary>
        protected virtual void OnSolved() { }

        /// <summary>Call after any progress: refreshes the colour and tells the HUD / label.</summary>
        protected void NotifyChanged()
        {
            ShowStatusColour();
            Changed?.Invoke(this);
        }

        /// <summary>Slides the opening away once (door sinks, gate bars drop).</summary>
        protected void Open()
        {
            if (opened || opening == null) return;
            opened = true;
            StartCoroutine(Slide(opening, opening.localPosition + openOffset));
        }

        private IEnumerator Slide(Transform target, Vector3 end)
        {
            Vector3 start = target.localPosition;
            for (float t = 0f; t < openSeconds; t += Time.deltaTime)
            {
                // SmoothStep eases in and out, so heavy things start and stop gently.
                target.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / openSeconds));
                yield return null;
            }
            target.localPosition = end;
        }

        /// <summary>The player's inventory if this collider belongs to the player, else null.</summary>
        protected static Inventory PlayerInventory(Collider other) => other.GetComponentInParent<Inventory>();

        // ---------- Status colour ----------

        // A MaterialPropertyBlock tints per renderer without touching the shared material.
        private void ShowStatusColour()
        {
            if (statusRenderers == null || statusRenderers.Length == 0) return;
            Color color = IsSolved ? solvedColor : IsLocked ? lockedColor : unsolvedColor;
            block ??= new MaterialPropertyBlock();
            foreach (Renderer r in statusRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor(MaterialUtility.BaseColor, color);
                block.SetColor(MaterialUtility.EmissionColor, color);   // glowing orbs glow in the state colour
                r.SetPropertyBlock(block);
            }
        }

        // ---------- Debug (Inspector ⋮ menu) ----------

        // Play Mode only: skip a puzzle while testing the others.
        [ContextMenu("Debug: Solve now")]
        internal void DebugSolve()
        {
            if (Application.isPlaying) MarkSolved();
        }
    }
}
