using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// M shows / hides the minimap (PB-24, ADR-0010). The Gameplay input map is only enabled while
    /// playing, so M does nothing on the intro / end screens. While hidden, the minimap camera is
    /// switched off too (no render cost). Lives on the HUD, not on the panel it hides
    /// (a disabled object would stop listening).
    /// </summary>
    public class MinimapToggle : MonoBehaviour
    {
        [Tooltip("Input source (M = ToggleMinimap).")]
        [SerializeField] private GameInput input;
        [Tooltip("The minimap panel to show / hide.")]
        [SerializeField] private GameObject panel;
        [Tooltip("The minimap camera (disabled while hidden).")]
        [SerializeField] private Camera minimapCamera;
        [Tooltip("Shown when the game starts.")]
        [SerializeField] private bool visibleAtStart = true;

        private void Start() => SetVisible(visibleAtStart);

        private void OnEnable()
        {
            if (input != null) input.ToggleMinimapPressed += Toggle;
            else Debug.LogError($"{nameof(MinimapToggle)}: Input is not assigned; M won't work.", this);
        }

        private void OnDisable()
        {
            if (input != null) input.ToggleMinimapPressed -= Toggle;
        }

        // RUBRIC 5b · Minimap toggle (5): M shows / hides the minimap.
        private void Toggle() => SetVisible(panel == null || !panel.activeSelf);

        private void SetVisible(bool visible)
        {
            if (panel != null) panel.SetActive(visible);
            if (minimapCamera != null) minimapCamera.enabled = visible;
        }
    }
}
