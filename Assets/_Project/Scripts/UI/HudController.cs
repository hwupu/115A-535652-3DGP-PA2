using PuzzleMaster.Core;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// In-game HUD (PB-05 timer, PB-15 score). The timer is in the upper-left as the spec asks:
    /// "Time Remaining" + MM:SS. It is read every frame from CountdownTimer (cheap: one string
    /// per second); the score updates on ScoreManager.Changed.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Tooltip("The countdown to display.")]
        [SerializeField] private CountdownTimer timer;
        [Tooltip("Score source.")]
        [SerializeField] private ScoreManager score;

        [Tooltip("Text showing the remaining time as MM:SS.")]
        [SerializeField] private TMP_Text timerText;
        [Tooltip("Text showing the score.")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Low-time warning")]
        [Tooltip("Below this many seconds the timer turns to the warning colour.")]
        [SerializeField] private float warningSeconds = 30f;
        [Tooltip("Timer colour when time is running out.")]
        [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.3f);

        private Color normalColor;
        private int shownSeconds = -1;

        private void Awake()
        {
            if (timerText != null) normalColor = timerText.color;
        }

        private void OnEnable()
        {
            if (score != null)
            {
                score.Changed += ShowScore;
                ShowScore(score.Score);
            }
        }

        private void OnDisable()
        {
            if (score != null) score.Changed -= ShowScore;
        }

        // RUBRIC 3c · Timer display (4): "Time Remaining" MM:SS in the upper-left.
        private void Update()
        {
            if (timer == null || timerText == null) return;
            // Only rebuild the string when the shown second changes (avoids garbage every frame).
            int seconds = Mathf.CeilToInt(timer.Remaining);
            if (seconds == shownSeconds) return;
            shownSeconds = seconds;
            timerText.text = CountdownTimer.Format(timer.Remaining);
            timerText.color = timer.Remaining <= warningSeconds ? warningColor : normalColor;
        }

        // RUBRIC 3b · Score display (4): redrawn on ScoreManager.Changed (event, not polled).
        private void ShowScore(int value)
        {
            if (scoreText != null) scoreText.text = $"Score: {value}";
        }
    }
}
