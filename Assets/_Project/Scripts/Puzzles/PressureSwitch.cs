using System;
using PuzzleMaster.Core;
using PuzzleMaster.Items;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// A floor switch for the switch puzzle (PB-20). Stepping on it activates it for good (it latches,
    /// so the player doesn't have to stand on two switches at once). The pad sinks a little and its
    /// indicator turns from the off to the on colour. Its SwitchPuzzle listens to <see cref="Activated"/>.
    /// Needs a trigger collider and a kinematic Rigidbody on this object (set up by Setup → Sprint 3).
    /// </summary>
    public class PressureSwitch : MonoBehaviour
    {
        [Tooltip("The part that sinks when stepped on.")]
        [SerializeField] private Transform pad;
        [Tooltip("How far the pad sinks, in metres.")]
        [SerializeField] private float pressDepth = 0.08f;
        [Tooltip("Renderer tinted off / on (usually the pad).")]
        [SerializeField, OptionalReference] private Renderer indicator;
        [SerializeField] private Color offColor = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private Color onColor = new Color(0.25f, 1f, 0.35f);

        /// <summary>Raised once, when the switch is first stepped on.</summary>
        public event Action<PressureSwitch> Activated;

        public bool IsActive { get; private set; }

        private void Start() => Tint(offColor);

        private void OnTriggerEnter(Collider other)
        {
            if (IsActive || other.GetComponentInParent<Inventory>() == null) return;   // only the player
            IsActive = true;
            if (pad != null) pad.localPosition += Vector3.down * pressDepth;
            Tint(onColor);
            Activated?.Invoke(this);
        }

        private void Tint(Color color)
        {
            if (indicator == null) return;
            var block = new MaterialPropertyBlock();
            indicator.GetPropertyBlock(block);
            block.SetColor(MaterialUtility.BaseColor, color);
            block.SetColor(MaterialUtility.EmissionColor, color);
            indicator.SetPropertyBlock(block);
        }
    }
}
