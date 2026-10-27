using PuzzleMaster.Core;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// A one-line message near the bottom of the screen (PB-17): puzzle hints ("you need the Key"),
    /// progress and "solved!". Listens to GameEvents.PuzzleMessage. A new message goes below the
    /// previous one while that is still showing (at most two lines, e.g. "Pedestal solved! +200" and
    /// "The gate is open!"); the text stays for a few seconds after the last message, then fades out.
    /// </summary>
    public class HudMessage : MonoBehaviour
    {
        [Tooltip("Text that shows the message.")]
        [SerializeField] private TMP_Text text;
        [Tooltip("Seconds a message stays fully visible.")]
        [SerializeField] private float holdSeconds = 3f;
        [Tooltip("Seconds the fade-out takes.")]
        [SerializeField] private float fadeSeconds = 0.6f;

        private float shownAt = float.NegativeInfinity;
        private string previous = "";

        private void OnEnable()
        {
            if (text == null) Debug.LogError($"{nameof(HudMessage)}: Text is not assigned.", this);
            GameEvents.PuzzleMessage += Show;
        }

        private void OnDisable() => GameEvents.PuzzleMessage -= Show;

        private void Start()
        {
            if (text != null) text.alpha = 0f;
        }

        private void Show(string message)
        {
            if (text == null) return;
            bool stillShowing = Time.time - shownAt <= holdSeconds;
            text.text = stillShowing && previous.Length > 0 ? $"{previous}\n{message}" : message;
            previous = message;
            shownAt = Time.time;
        }

        private void Update()
        {
            if (text == null) return;
            float age = Time.time - shownAt;
            text.alpha = age <= holdSeconds ? 1f : Mathf.Clamp01(1f - (age - holdSeconds) / fadeSeconds);
        }
    }
}
