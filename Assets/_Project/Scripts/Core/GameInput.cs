using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Single entry point for input (ADR-0003, PB-02…PB-08).
    /// Wraps PlayerControls.inputactions, so other scripts never touch the Input System directly.
    /// Three action maps, enabled by game state (GameStateManager calls the Set…Enabled methods):
    /// - Gameplay (only while Playing): Move, Look, Attack, ToggleMinimap, OrbitHold
    /// - Screens  (intro / win / lose):   Start (SPACE), Restart (R)
    /// - Global   (always):               Quit (ESC), ReloadConfig (F5)
    /// Continuous values (Move, Look) are polled; buttons are C# events. A disabled map reads
    /// zero and raises nothing, so gameplay scripts need no "are we playing?" checks.
    /// Lives in Core (not Player) because Core systems (game state, config) need input too.
    /// </summary>
    public class GameInput : MonoBehaviour
    {
        public const string GameplayMap = "Gameplay";
        public const string ScreensMap = "Screens";
        public const string GlobalMap = "Global";

        [Tooltip("PlayerControls.inputactions (Assets/_Project/Settings).")]
        [SerializeField] private InputActionAsset actions;

        // Button events. Listeners subscribe in their OnEnable and unsubscribe in OnDisable.
        public event Action AttackPressed;
        public event Action ToggleMinimapPressed;
        public event Action StartPressed;
        public event Action RestartPressed;
        public event Action QuitPressed;
        public event Action ReloadConfigPressed;

        /// <summary>WASD as (x = strafe, y = forward), each in [-1, 1]. Zero outside gameplay.</summary>
        public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Mouse movement this frame, in pixels. Zero outside gameplay.</summary>
        public Vector2 Look => look != null ? look.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>
        /// True while the orbit button is held (middle mouse or Left Alt): the camera circles the player
        /// instead of turning it, e.g. to show the character's front in the demo. False outside gameplay.
        /// </summary>
        public bool IsOrbitHeld => orbitHold != null && orbitHold.IsPressed();

        private InputActionMap gameplay, screens, global;
        private InputAction move, look, orbitHold, attack, toggleMinimap, start, restart, quit, reloadConfig;

        // Awake runs before any OnEnable/Start, so the actions are looked up once before others poll them.
        // throwIfNotFound: a typo in a map or action name fails loudly instead of silently reading zero.
        private void Awake()
        {
            if (actions == null)
            {
                Debug.LogError($"{nameof(GameInput)}: no InputActionAsset assigned. Run Puzzle Master → Setup → Sprint 1.", this);
                enabled = false;
                return;
            }

            gameplay = actions.FindActionMap(GameplayMap, true);
            screens = actions.FindActionMap(ScreensMap, true);
            global = actions.FindActionMap(GlobalMap, true);
            move = gameplay.FindAction("Move", true);
            look = gameplay.FindAction("Look", true);
            // Added in Sprint 4: an asset made before that has no OrbitHold until Setup → Sprint 4 runs.
            orbitHold = gameplay.FindAction("OrbitHold");
            if (orbitHold == null)
                Debug.LogWarning($"{nameof(GameInput)}: no OrbitHold action; the orbit camera is off. Run Puzzle Master → Setup → Sprint 4.", this);
            attack = gameplay.FindAction("Attack", true);
            toggleMinimap = gameplay.FindAction("ToggleMinimap", true);
            start = screens.FindAction("Start", true);
            restart = screens.FindAction("Restart", true);
            quit = global.FindAction("Quit", true);
            reloadConfig = global.FindAction("ReloadConfig", true);
        }

        private void OnEnable()
        {
            if (global == null) return;

            // "performed" fires once per button press. The On… methods forward it to our C# events
            // ("?." = only if someone is subscribed). Named methods, not lambdas, so OnDisable can remove them.
            attack.performed += OnAttack;
            toggleMinimap.performed += OnToggleMinimap;
            start.performed += OnStart;
            restart.performed += OnRestart;
            quit.performed += OnQuit;
            reloadConfig.performed += OnReloadConfig;
            global.Enable();   // Gameplay / Screens are enabled by GameStateManager per state
        }

        private void OnDisable()
        {
            if (global == null) return;

            // Mirror of OnEnable. The asset is shared, so disable every map we may have enabled
            // (a scene reload creates a new GameInput that enables them again).
            gameplay.Disable();
            screens.Disable();
            global.Disable();
            attack.performed -= OnAttack;
            toggleMinimap.performed -= OnToggleMinimap;
            start.performed -= OnStart;
            restart.performed -= OnRestart;
            quit.performed -= OnQuit;
            reloadConfig.performed -= OnReloadConfig;
        }

        /// <summary>Turns the Gameplay map (move, look, attack, minimap) on or off.</summary>
        public void SetGameplayEnabled(bool value) => SetMap(gameplay, value);

        /// <summary>Turns the Screens map (SPACE start, R restart) on or off.</summary>
        public void SetScreensEnabled(bool value) => SetMap(screens, value);

        private static void SetMap(InputActionMap map, bool value)
        {
            if (map == null) return;
            if (value) map.Enable();
            else map.Disable();
        }

        private void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
        private void OnToggleMinimap(InputAction.CallbackContext _) => ToggleMinimapPressed?.Invoke();
        private void OnStart(InputAction.CallbackContext _) => StartPressed?.Invoke();
        private void OnRestart(InputAction.CallbackContext _) => RestartPressed?.Invoke();
        private void OnQuit(InputAction.CallbackContext _) => QuitPressed?.Invoke();
        private void OnReloadConfig(InputAction.CallbackContext _) => ReloadConfigPressed?.Invoke();
    }
}
