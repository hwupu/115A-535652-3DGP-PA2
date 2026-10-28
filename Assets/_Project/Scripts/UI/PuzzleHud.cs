using System.Text;
using PuzzleMaster.Puzzles;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// HUD objective list (PB-17): "Puzzles x/5" and one line per puzzle with its status,
    /// solved ones in green. Redraws only when a puzzle changes.
    /// </summary>
    public class PuzzleHud : MonoBehaviour
    {
        [Tooltip("Puzzle source.")]
        [SerializeField] private PuzzleManager manager;
        [Tooltip("Text that shows the list.")]
        [SerializeField] private TMP_Text text;
        [Tooltip("Colour of a solved puzzle's line.")]
        [SerializeField] private Color solvedColor = new Color(0.4f, 1f, 0.45f);
        [Tooltip("Colour of an unsolved puzzle's line.")]
        [SerializeField] private Color openColor = new Color(0.85f, 0.85f, 0.85f);

        private readonly StringBuilder builder = new StringBuilder();

        private void OnEnable()
        {
            if (manager == null)
            {
                Debug.LogError($"{nameof(PuzzleHud)}: Manager is not assigned.", this);
                return;
            }
            manager.Changed += Redraw;
            Redraw();
        }

        private void OnDisable()
        {
            if (manager != null) manager.Changed -= Redraw;
        }

        private void Redraw()
        {
            if (text == null) return;
            string solved = ColorUtility.ToHtmlStringRGB(solvedColor), open = ColorUtility.ToHtmlStringRGB(openColor);
            builder.Clear().Append("Puzzles ").Append(manager.SolvedCount).Append('/').Append(manager.Total);
            foreach (Puzzle p in manager.Puzzles)
            {
                if (p == null) continue;
                // TMP rich text: a coloured line, the status in a smaller size.
                builder.Append("\n<color=#").Append(p.IsSolved ? solved : open).Append('>')
                       .Append(p.IsSolved ? "[x] " : "[  ] ").Append(p.Title)
                       .Append("  <size=80%>").Append(p.Status).Append("</size></color>");
            }
            text.text = builder.ToString();
        }
    }
}
