using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// Solved when all of its pressure switches are active (PB-20: two switches in the world).
    /// The switches can stand anywhere; this component only counts them.
    /// </summary>
    public class SwitchPuzzle : Puzzle
    {
        [Tooltip("The switches that must all be stepped on.")]
        [SerializeField] private PressureSwitch[] switches = new PressureSwitch[0];

        private int ActiveCount
        {
            get
            {
                int n = 0;
                foreach (PressureSwitch s in switches)
                    if (s != null && s.IsActive) n++;
                return n;
            }
        }

        public override string Status => IsSolved ? "solved" : $"Switches {ActiveCount}/{switches.Length}";

        private void OnEnable()
        {
            if (switches.Length == 0) Debug.LogError($"{name}: {nameof(SwitchPuzzle)} has no switches; it can't be solved.", this);
            foreach (PressureSwitch s in switches)
                if (s != null) s.Activated += OnSwitchActivated;
        }

        private void OnDisable()
        {
            foreach (PressureSwitch s in switches)
                if (s != null) s.Activated -= OnSwitchActivated;
        }

        private void OnSwitchActivated(PressureSwitch _)
        {
            if (IsSolved) return;
            GameEvents.RaisePuzzleProgressed(transform.position);
            if (ActiveCount >= switches.Length) MarkSolved();
            else
            {
                GameEvents.RaisePuzzleMessage($"{Title}: {ActiveCount}/{switches.Length} switches active.");
                NotifyChanged();
            }
        }
    }
}
