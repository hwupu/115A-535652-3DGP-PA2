using PuzzleMaster.Puzzles;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// A floating label above a puzzle site: its title and status, e.g. "Crystal Altar / Crystals 1/3"
    /// (Sprint 2 retro: placeholders must explain themselves in the scene). It faces the camera.
    /// Hide it (untick the GameObject) once the final art explains the puzzle by itself.
    /// </summary>
    public class PuzzleLabel : MonoBehaviour
    {
        [Tooltip("The puzzle to describe.")]
        [SerializeField] private Puzzle puzzle;
        [Tooltip("World-space TextMeshPro text.")]
        [SerializeField] private TMP_Text text;

        private Transform cameraTransform;

        private void OnEnable()
        {
            if (puzzle == null || text == null)
            {
                Debug.LogError($"{nameof(PuzzleLabel)} on {name}: Puzzle or Text is not assigned.", this);
                return;
            }
            puzzle.Changed += Redraw;
            Redraw(puzzle);
        }

        private void OnDisable()
        {
            if (puzzle != null) puzzle.Changed -= Redraw;
        }

        private void Redraw(Puzzle p) => text.text = $"{p.Title}\n<size=65%>{p.Status}</size>";

        // Same as the life bars: copy the camera's rotation so the text stays parallel to the screen.
        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                Camera main = Camera.main;
                if (main == null) return;
                cameraTransform = main.transform;
            }
            transform.rotation = cameraTransform.rotation;
        }
    }
}
