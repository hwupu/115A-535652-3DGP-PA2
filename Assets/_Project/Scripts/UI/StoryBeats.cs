using System.Collections;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// Hero's Journey framing during play (PB-27; story from the spec). Posts short story lines to
    /// the HUD message line: "crossing the threshold" when gameplay starts, and a freed guardian
    /// when a monster is recovered. The intro prologue and the win / lose epilogues are static texts
    /// on the screens (Setup → Sprint 4); the "Crystal Seal restored" line comes with each puzzle
    /// solve (PuzzleManager). Texts are Inspector fields, so the PO can rewrite them without code.
    /// </summary>
    public class StoryBeats : MonoBehaviour
    {
        [Tooltip("Game state source: the start line is posted when gameplay begins.")]
        [SerializeField] private GameStateManager gameState;

        [Tooltip("Shown when gameplay starts (SPACE or R). Empty = nothing.")]
        [SerializeField, TextArea] private string startLine = "Restore the five Crystal Seals before the corruption becomes permanent!";
        [Tooltip("Shown when a monster is recovered. Empty = nothing.")]
        [SerializeField, TextArea] private string recoveredLine = "A guardian is freed from the dark magic.";

        private void OnEnable()
        {
            if (gameState != null) gameState.StateChanged += OnStateChanged;
            else Debug.LogError($"{nameof(StoryBeats)}: Game State is not assigned; the start line won't show.", this);
            GameEvents.MonsterRecovered += OnMonsterRecovered;
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.StateChanged -= OnStateChanged;
            GameEvents.MonsterRecovered -= OnMonsterRecovered;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Playing) StartCoroutine(PostNextFrame(startLine));
        }

        // The HUD (and its message line) is switched on by the same state change, and its Start
        // clears the text; posting one frame later makes sure the line is not hidden right away.
        private static IEnumerator PostNextFrame(string line)
        {
            yield return null;
            Post(line);
        }

        private void OnMonsterRecovered(Vector3 _) => Post(recoveredLine);

        private static void Post(string line)
        {
            if (!string.IsNullOrEmpty(line)) GameEvents.RaisePuzzleMessage(line);
        }
    }
}
