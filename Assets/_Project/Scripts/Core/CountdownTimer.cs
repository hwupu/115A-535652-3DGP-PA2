using System;
using UnityEngine;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// The 5-minute countdown (PB-05). Counts down only while running; GameStateManager starts it
    /// when gameplay begins and stops it on win / lose, and listens to <see cref="Expired"/> to lose.
    /// Uses scaled time, so it also freezes whenever Time.timeScale is 0.
    /// The length comes from config.json (timeLimitSeconds) or the Inspector fallback.
    /// </summary>
    public class CountdownTimer : MonoBehaviour
    {
        [Tooltip("Countdown length in seconds when no config.json is loaded. Spec: 300 (5 minutes).")]
        [SerializeField] private float timeLimitSeconds = 300f;

        /// <summary>Raised once when the countdown reaches zero.</summary>
        public event Action Expired;

        /// <summary>Seconds left (never below 0).</summary>
        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }

        private float Limit => ConfigLoader.Current?.timeLimitSeconds ?? timeLimitSeconds;

        private void Awake() => Remaining = Limit;

        // A config reload (F5) before the countdown starts updates the shown start time;
        // during gameplay it is ignored, so the running clock never jumps.
        private void OnEnable() => ConfigLoader.Changed += OnConfigChanged;
        private void OnDisable() => ConfigLoader.Changed -= OnConfigChanged;

        private void OnConfigChanged(GameConfig _)
        {
            if (!IsRunning && Remaining > 0f) Remaining = Limit;
        }

        /// <summary>Starts (or restarts) the countdown from the full time limit.</summary>
        public void StartCountdown()
        {
            Remaining = Limit;
            IsRunning = true;
        }

        /// <summary>Freezes the countdown at its current value.</summary>
        public void Stop() => IsRunning = false;

        private void Update()
        {
            if (!IsRunning) return;
            Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
            if (Remaining > 0f) return;

            IsRunning = false;
            Expired?.Invoke();
        }

        /// <summary>
        /// Formats seconds as MM:SS (spec format). Rounds up, so the clock shows 05:00 at the start
        /// and reaches 00:00 exactly when time runs out.
        /// </summary>
        public static string Format(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
