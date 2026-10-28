using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Owns the game flow (ADR-0006, PB-04 … PB-07):
    ///   Intro —SPACE→ Playing —all puzzles solved→ Won
    ///                         —timer reaches 0→   Lost
    ///   Won / Lost —R→ reload the scene straight into Playing (no intro, PO decision)
    ///   any state  —ESC→ quit
    /// Every state change sets time scale, input maps, cursor and timer in one place
    /// (<see cref="Enter"/>), then raises <see cref="StateChanged"/> for UI, audio and others.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Tooltip("Input source (SPACE / R / ESC, and the Gameplay map switch).")]
        [SerializeField] private GameInput input;

        [Tooltip("The countdown; reaching zero means Lost.")]
        [SerializeField] private CountdownTimer timer;

        /// <summary>Raised after every state change (also for the first state in Start).</summary>
        public event Action<GameState> StateChanged;

        public GameState State { get; private set; } = GameState.Intro;

        // Survives the scene reload on R (statics are not reset by LoadScene), so the reloaded
        // scene starts in Playing instead of Intro. Reset when the domain reloads (see below).
        private static bool skipIntroOnce;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => skipIntroOnce = false;   // for "Enter Play Mode" without domain reload

        // ---------- Setup ----------

        private void OnEnable()
        {
            if (input != null)
            {
                input.StartPressed += OnStartPressed;
                input.RestartPressed += OnRestartPressed;
                input.QuitPressed += Quit;
            }
            else Debug.LogError($"{nameof(GameStateManager)}: Input is not assigned; SPACE / R / ESC won't work.", this);

            if (timer != null) timer.Expired += OnTimerExpired;
            else Debug.LogError($"{nameof(GameStateManager)}: Timer is not assigned; the game can't be lost.", this);
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.StartPressed -= OnStartPressed;
                input.RestartPressed -= OnRestartPressed;
                input.QuitPressed -= Quit;
            }
            if (timer != null) timer.Expired -= OnTimerExpired;
        }

        // RUBRIC 3a · Introduction screen (4): the game starts in Intro (frozen world) until SPACE.
        // Start (not Awake) so every listener has subscribed in its OnEnable before the first event.
        private void Start()
        {
            GameState first = skipIntroOnce ? GameState.Playing : GameState.Intro;
            skipIntroOnce = false;
            Enter(first);
        }

        // ---------- Transitions (who may change the state, and when) ----------

        /// <summary>Called by PuzzleManager when all five puzzles are solved (PB-17).</summary>
        public void Win()
        {
            if (State == GameState.Playing) Enter(GameState.Won);
        }

        private void OnStartPressed()
        {
            if (State == GameState.Intro) Enter(GameState.Playing);
        }

        // RUBRIC 3e · Win / lose screens (4): timer at 0 → Lost ("Time's Up!"). Win comes from PuzzleManager → Win().
        private void OnTimerExpired()
        {
            if (State == GameState.Playing) Enter(GameState.Lost);
        }

        // Spec control: R restarts from the win / lose screen (reloads the scene).
        private void OnRestartPressed()
        {
            if (State != GameState.Won && State != GameState.Lost) return;
            skipIntroOnce = true;
            Time.timeScale = 1f;   // timeScale is global and survives LoadScene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Spec control: ESC quits (Global input map, works in every state).
        /// <summary>ESC: quits the app; in the Editor, stops Play Mode instead.</summary>
        public void Quit()
        {
            Debug.Log("Quit (ESC)");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- Entering a state ----------

        /// <summary>Applies everything that depends on the state, then announces it.</summary>
        private void Enter(GameState state)
        {
            State = state;
            bool playing = state == GameState.Playing;

            // Frozen world outside gameplay: monsters, spells and physics stop; UI still updates.
            Time.timeScale = playing ? 1f : 0f;

            if (input != null)
            {
                input.SetGameplayEnabled(playing);
                input.SetScreensEnabled(!playing);
            }

            // Mouse look needs a locked, hidden cursor; screens get a free cursor (ADR-0007).
            Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !playing;

            if (timer != null)
            {
                if (playing) timer.StartCountdown();
                else timer.Stop();
            }

            Debug.Log($"Game state → {state}");
            StateChanged?.Invoke(state);
        }

        // ---------- Debug (Inspector ⋮ menu) ----------

        // Play Mode only: jump to the end screens without playing through.
        // They also work from the Intro screen, where the cursor is free to click the Inspector.
        [ContextMenu("Debug: Win now")]
        private void DebugWin()
        {
            if (State == GameState.Intro || State == GameState.Playing) Enter(GameState.Won);
        }

        [ContextMenu("Debug: Lose now")]
        private void DebugLose()
        {
            if (State == GameState.Intro || State == GameState.Playing) Enter(GameState.Lost);
        }
    }
}
