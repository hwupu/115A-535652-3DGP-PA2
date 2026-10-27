using PuzzleMaster.Core;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// Shows the panel that belongs to the current game state (ADR-0006, PB-04 / PB-06):
    /// Intro → intro panel; Playing → HUD; Won / Lost → HUD + the win or lose panel.
    /// On win / lose it also fills in the final score (and the time left on a win).
    /// The panels' fixed texts (spec wording) are authored in the scene by the setup script,
    /// so the PO can restyle them without code changes.
    /// </summary>
    public class GameScreens : MonoBehaviour
    {
        [Tooltip("Game state source.")]
        [SerializeField] private GameStateManager gameState;
        [Tooltip("Score for the end screens.")]
        [SerializeField] private ScoreManager score;
        [Tooltip("Time left for the win screen.")]
        [SerializeField] private CountdownTimer timer;

        [Header("Panels")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("End screen results")]
        [Tooltip("Text on the win panel for score and time left.")]
        [SerializeField] private TMP_Text winResultText;
        [Tooltip("Text on the lose panel for the score.")]
        [SerializeField] private TMP_Text loseResultText;

        private void OnEnable()
        {
            if (gameState == null)
            {
                Debug.LogError($"{nameof(GameScreens)}: Game State is not assigned; screens won't change.", this);
                return;
            }
            gameState.StateChanged += Show;
            Show(gameState.State);   // in case the first state was announced before we subscribed
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.StateChanged -= Show;
        }

        private void Show(GameState state)
        {
            SetActive(introPanel, state == GameState.Intro);
            SetActive(hudPanel, state != GameState.Intro);
            SetActive(winPanel, state == GameState.Won);
            SetActive(losePanel, state == GameState.Lost);

            int points = score != null ? score.Score : 0;
            if (state == GameState.Won && winResultText != null)
            {
                string timeLeft = timer != null ? CountdownTimer.Format(timer.Remaining) : "--:--";
                winResultText.text = $"Final Score: {points}\nTime Left: {timeLeft}";
            }
            if (state == GameState.Lost && loseResultText != null)
                loseResultText.text = $"Final Score: {points}";
        }

        private static void SetActive(GameObject panel, bool active)
        {
            if (panel != null && panel.activeSelf != active) panel.SetActive(active);
        }
    }
}
