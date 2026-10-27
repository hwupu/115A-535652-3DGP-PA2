using System.IO;
using PuzzleMaster.Cameras;
using PuzzleMaster.Core;
using PuzzleMaster.Player;
using PuzzleMaster.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 1 setup (ADR-0005; PB-01 … PB-08). Idempotent: safe to run again; it only creates
    /// what is missing and never overwrites references or values you already set.
    /// It builds the walking skeleton and game flow in Main.unity:
    /// - Input: PlayerControls.inputactions (Gameplay / Screens / Global maps, ADR-0003); removes the
    ///   template's InputSystem_Actions. Imports the TextMeshPro essentials if missing.
    /// - Greybox region (80 × 80 m): ground, boundary walls, a few props, five puzzle-site markers
    ///   and five monster-spot markers for Sprints 2 and 3 (PB-01).
    /// - Player: CharacterController + PlayerMotor on a "Player" layer, capsule + nose visuals (PB-02).
    /// - Main Camera: CameraRig following the player (PB-03).
    /// - Systems/GameManager: GameInput, ConfigLoader (F5), GameStateManager, CountdownTimer, ScoreManager.
    /// - UI/Screens canvas: HUD (timer upper-left, score), intro / win / lose panels with the
    ///   spec's wording, and GameScreens to switch them (PB-04 … PB-06).
    /// Menu: Puzzle Master → Setup → Sprint 1 (Walking Skeleton)
    /// Batch: Unity -batchmode -quit -projectPath . -executeMethod PuzzleMaster.EditorTools.Sprint1Setup.RunBatch
    /// </summary>
    public static class Sprint1Setup
    {
        public const string InputActionsPath = SetupUtils.ProjectRoot + "/Settings/PlayerControls.inputactions";
        private const string TemplateInputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string ProjectWideActionsKey = "com.unity.input.settings.actions";
        private const string TmpEssentialsPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        internal const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const float RegionSize = 80f;       // ground is RegionSize × RegionSize metres
        private const float WallHeight = 3f;
        private static readonly Vector3 PlayerStart = new Vector3(0f, 0f, -30f);

        // Five puzzle sites (spec order) and five monster spots. Sprint 2 / 3 setups build on these positions.
        private static readonly (string name, Vector3 position)[] PuzzleSites =
        {
            ("PuzzleSite_1_Key", new Vector3(-25f, 0f, 15f)),
            ("PuzzleSite_2_Crystals", new Vector3(25f, 0f, 15f)),
            ("PuzzleSite_3_Switches", new Vector3(-25f, 0f, -15f)),
            ("PuzzleSite_4_Artifact", new Vector3(25f, 0f, -15f)),
            ("PuzzleSite_5_FinalGate", new Vector3(0f, 0f, 33f)),
        };
        private static readonly Vector3[] MonsterSpots =
        {
            new Vector3(-15f, 0f, 25f), new Vector3(15f, 0f, 25f), new Vector3(-30f, 0f, 0f),
            new Vector3(30f, 0f, 0f), new Vector3(0f, 0f, 5f),
        };
        private static readonly (Vector3 position, Vector3 scale)[] Props =
        {
            (new Vector3(-10f, 1f, -20f), new Vector3(2f, 2f, 2f)),
            (new Vector3(12f, 1.5f, -5f), new Vector3(1.5f, 3f, 1.5f)),
            (new Vector3(-6f, 1f, 18f), new Vector3(3f, 2f, 1.5f)),
            (new Vector3(20f, 1f, 30f), new Vector3(2f, 2f, 2f)),
            (new Vector3(-34f, 1f, -32f), new Vector3(2.5f, 2f, 2.5f)),
            (new Vector3(34f, 1.5f, 32f), new Vector3(1.5f, 3f, 1.5f)),
            (new Vector3(8f, 1f, -32f), new Vector3(3f, 2f, 2f)),
        };

        /// <summary>Menu entry: refuses to run in Play Mode and lets the user save open scene changes first.</summary>
        [MenuItem("Puzzle Master/Setup/Sprint 1 (Walking Skeleton)")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Sprint 1 setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string log = Run();
            EditorUtility.DisplayDialog("Sprint 1 setup finished",
                "Done. See the Console for the full list of created / linked items.\n\nNext: press Play and follow the checklist in docs/guides/sprint-01-setup.md.", "OK");
            Debug.Log(log);
        }

        /// <summary>Entry point for -executeMethod (no dialogs, so it works in -batchmode).</summary>
        public static void RunBatch() => Debug.Log(Run());

        private static string Run()
        {
            var u = new SetupUtils();

            // Open the scene first: OpenScene unloads unused assets, which would invalidate
            // references to assets created earlier in this run.
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != Sprint0Setup.MainScenePath)
                scene = EditorSceneManager.OpenScene(Sprint0Setup.MainScenePath, OpenSceneMode.Single);

            // ---------- Project assets ----------
            int playerLayer = u.EnsureLayer("Player");
            InputActionAsset actions = GetOrCreateInputActions(u);
            RemoveTemplateInputActions(u);
            TMP_FontAsset font = EnsureTmpEssentials(u);

            Material mGround = u.GetOrCreateMaterial("M_Ground", new Color(0.36f, 0.42f, 0.32f));
            Material mWall = u.GetOrCreateMaterial("M_Wall", new Color(0.45f, 0.45f, 0.5f));
            Material mProp = u.GetOrCreateMaterial("M_Prop", new Color(0.5f, 0.44f, 0.38f));
            Material mPuzzleSite = u.GetOrCreateMaterial("M_PuzzleSite", new Color(0.25f, 0.55f, 0.95f));
            Material mMonsterSpot = u.GetOrCreateMaterial("M_MonsterSpot", new Color(0.85f, 0.25f, 0.25f));
            Material mPlayer = u.GetOrCreateMaterial("M_Player", new Color(0.2f, 0.6f, 0.9f));
            Material mPlayerNose = u.GetOrCreateMaterial("M_PlayerNose", new Color(0.95f, 0.8f, 0.2f));

            // ---------- Region (PB-01) ----------
            Transform environment = u.FindOrCreate(scene, "Environment").transform;
            float half = RegionSize / 2f;
            // A Plane primitive is 10 × 10 m at scale 1.
            u.FindOrCreatePrimitive(environment, "Ground", PrimitiveType.Plane, Vector3.zero,
                new Vector3(RegionSize / 10f, 1f, RegionSize / 10f), mGround);

            Transform boundary = u.FindOrCreate(scene, "Environment/Boundary").transform;
            float wallY = WallHeight / 2f, wallOffset = half + 0.5f, wallLength = RegionSize + 2f;
            u.FindOrCreatePrimitive(boundary, "Wall_North", PrimitiveType.Cube, new Vector3(0f, wallY, wallOffset), new Vector3(wallLength, WallHeight, 1f), mWall);
            u.FindOrCreatePrimitive(boundary, "Wall_South", PrimitiveType.Cube, new Vector3(0f, wallY, -wallOffset), new Vector3(wallLength, WallHeight, 1f), mWall);
            u.FindOrCreatePrimitive(boundary, "Wall_East", PrimitiveType.Cube, new Vector3(wallOffset, wallY, 0f), new Vector3(1f, WallHeight, wallLength), mWall);
            u.FindOrCreatePrimitive(boundary, "Wall_West", PrimitiveType.Cube, new Vector3(-wallOffset, wallY, 0f), new Vector3(1f, WallHeight, wallLength), mWall);

            Transform props = u.FindOrCreate(scene, "Environment/Props").transform;
            for (int i = 0; i < Props.Length; i++)
                u.FindOrCreatePrimitive(props, $"Prop_{i + 1:00}", PrimitiveType.Cube, Props[i].position, Props[i].scale, mProp);

            // Markers: flat discs without colliders, only to show where Sprints 2 / 3 will put things.
            foreach ((string name, Vector3 position) in PuzzleSites)
            {
                Transform site = u.FindOrCreate(scene, $"Gameplay/PuzzleSites/{name}").transform;
                if (site.localPosition == Vector3.zero && site.childCount == 0) site.localPosition = position;
                u.FindOrCreatePrimitive(site, "Marker", PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(6f, 0.02f, 6f), mPuzzleSite, keepCollider: false);
            }
            for (int i = 0; i < MonsterSpots.Length; i++)
            {
                Transform spot = u.FindOrCreate(scene, $"Gameplay/MonsterSpots/MonsterSpot_{i + 1}").transform;
                if (spot.localPosition == Vector3.zero && spot.childCount == 0) spot.localPosition = MonsterSpots[i];
                u.FindOrCreatePrimitive(spot, "Marker", PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(1.5f, 0.02f, 1.5f), mMonsterSpot, keepCollider: false);
            }

            // ---------- Systems ----------
            GameObject manager = u.FindOrCreate(scene, "Systems/GameManager");
            var gameInput = u.GetOrAdd<GameInput>(manager);
            u.LinkIfEmpty(gameInput, "actions", actions);
            var config = u.GetOrAdd<ConfigLoader>(manager);
            u.LinkIfEmpty(config, "input", gameInput);
            var timer = u.GetOrAdd<CountdownTimer>(manager);
            var score = u.GetOrAdd<ScoreManager>(manager);
            var gameState = u.GetOrAdd<GameStateManager>(manager);
            u.LinkIfEmpty(gameState, "input", gameInput);
            u.LinkIfEmpty(gameState, "timer", timer);

            // ---------- Player (PB-02) ----------
            GameObject player = SetupPlayer(u, scene, gameInput, playerLayer, mPlayer, mPlayerNose);

            // ---------- Camera (PB-03) ----------
            Transform cameraTransform = SetupUtils.FindRoot(scene, "Cameras")?.Find("Main Camera");
            if (cameraTransform == null) u.Warn("Cameras/Main Camera not found; CameraRig not added");
            else
            {
                bool isNew = !cameraTransform.TryGetComponent(out CameraRig _);
                var rig = u.GetOrAdd<CameraRig>(cameraTransform.gameObject);
                u.LinkIfEmpty(rig, "target", player.transform);
                u.LinkIfEmpty(rig, "input", gameInput);
                // The camera must not collide with the player's own capsule.
                if (isNew) SetupUtils.SetValue(rig, "collisionMask", p => p.intValue = ~(1 << playerLayer));
            }

            // ---------- UI (PB-04 … PB-06) ----------
            // TextMeshPro labels need the essentials (font, shaders). Without them, skip the UI
            // rather than create broken labels; a rerun after the import builds it.
            if (font != null) SetupScreens(u, scene, font, gameState, score, timer);
            else u.Warn("UI skipped until the TextMeshPro essentials exist: run this setup again");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Sprint 1 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        // ================================================================== Input

        /// <summary>Loads PlayerControls.inputactions, or generates it from code (bindings per ADR-0003).</summary>
        private static InputActionAsset GetOrCreateInputActions(SetupUtils u)
        {
            var existing = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (existing != null)
            {
                u.Skipped(InputActionsPath);
                return existing;
            }

            // Build the asset in memory, then save it as the .inputactions JSON the importer reads.
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "PlayerControls";

            InputActionMap gameplay = asset.AddActionMap(GameInput.GameplayMap);
            gameplay.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2")
                .AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            gameplay.AddAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            gameplay.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            gameplay.AddAction("ToggleMinimap", InputActionType.Button, "<Keyboard>/m");
            Sprint4Setup.AddOrbitHold(gameplay);

            InputActionMap screens = asset.AddActionMap(GameInput.ScreensMap);
            screens.AddAction("Start", InputActionType.Button, "<Keyboard>/space");
            screens.AddAction("Restart", InputActionType.Button, "<Keyboard>/r");

            InputActionMap global = asset.AddActionMap(GameInput.GlobalMap);
            global.AddAction("Quit", InputActionType.Button, "<Keyboard>/escape");
            global.AddAction("ReloadConfig", InputActionType.Button, "<Keyboard>/f5");

            u.EnsureFolder(Path.GetDirectoryName(InputActionsPath).Replace('\\', '/'));
            File.WriteAllText(InputActionsPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(InputActionsPath);
            u.Created(InputActionsPath);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        }

        /// <summary>
        /// Deletes the template's InputSystem_Actions (its bindings conflict with the spec, e.g.
        /// Space = Jump) and clears it as the project-wide actions asset. GameInput holds our asset directly.
        /// </summary>
        private static void RemoveTemplateInputActions(SetupUtils u)
        {
            if (EditorBuildSettings.TryGetConfigObject(ProjectWideActionsKey, out Object projectWide)
                && (projectWide == null || AssetDatabase.GetAssetPath(projectWide) == TemplateInputActionsPath))
            {
                EditorBuildSettings.RemoveConfigObject(ProjectWideActionsKey);
                u.Removed("project-wide input actions setting (template asset)");
            }
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(TemplateInputActionsPath, AssetPathToGUIDOptions.OnlyExistingAssets))
                && AssetDatabase.DeleteAsset(TemplateInputActionsPath))
                u.Removed(TemplateInputActionsPath);
        }

        /// <summary>Imports TextMeshPro's essential resources (font, settings, shaders) once; returns the default font.</summary>
        private static TMP_FontAsset EnsureTmpEssentials(SetupUtils u)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (font != null)
            {
                u.Skipped("TextMeshPro essentials");
                return font;
            }
            // Same as Window → TextMeshPro → Import TMP Essential Resources, without the dialog.
            // "Packages/…" is a virtual path; ImportPackage needs the real file (in Library/PackageCache).
            // The import is queued and finishes after this method returns (in batch mode it never runs
            // before -quit), so this run continues without a font and the caller skips the UI.
            AssetDatabase.ImportPackage(FileUtil.GetPhysicalPath(TmpEssentialsPackage), false);
            u.Warn("TextMeshPro essentials were missing: import started (or use Window → TextMeshPro → Import TMP Essential Resources)");
            return null;
        }

        // ================================================================== Player

        private static GameObject SetupPlayer(SetupUtils u, Scene scene, GameInput gameInput, int playerLayer,
            Material body, Material nose)
        {
            bool isNew = SetupUtils.FindRoot(scene, "Gameplay")?.Find("Player") == null;
            GameObject player = u.FindOrCreate(scene, "Gameplay/Player");
            if (isNew) player.transform.localPosition = PlayerStart;   // pivot at the feet, facing north (+Z)

            bool newController = !player.TryGetComponent(out CharacterController _);
            var controller = u.GetOrAdd<CharacterController>(player);
            if (newController)
            {
                controller.height = 1.8f;
                controller.radius = 0.4f;
                controller.center = new Vector3(0f, 0.9f, 0f);   // so the root pivot stays at the feet
            }
            var motor = u.GetOrAdd<PlayerMotor>(player);
            u.LinkIfEmpty(motor, "input", gameInput);

            // Greybox visuals in a Model child (prefab convention): a capsule body and a "nose" showing the facing.
            Transform model = u.FindOrCreate(scene, "Gameplay/Player/Model").transform;
            u.FindOrCreatePrimitive(model, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), body, keepCollider: false);
            u.FindOrCreatePrimitive(model, "Nose", PrimitiveType.Cube, new Vector3(0f, 1.45f, 0.4f), new Vector3(0.25f, 0.15f, 0.3f), nose, keepCollider: false);

            if (player.layer != playerLayer)
            {
                SetupUtils.SetLayerRecursively(player, playerLayer);
                u.Linked($"Player → layer Player ({playerLayer})");
            }
            return player;
        }

        // ================================================================== UI

        private static void SetupScreens(SetupUtils u, Scene scene, TMP_FontAsset font,
            GameStateManager gameState, ScoreManager score, CountdownTimer timer)
        {
            GameObject root = u.FindOrCreate(scene, "UI/Screens");
            bool newCanvas = !root.TryGetComponent(out Canvas _);
            var canvas = u.GetOrAdd<Canvas>(root);
            var scaler = u.GetOrAdd<CanvasScaler>(root);
            if (newCanvas)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            Transform parent = root.transform;
            var overlay = new Color(0f, 0f, 0f, 0.75f);
            Vector2 topLeft = new Vector2(0f, 1f), center = new Vector2(0.5f, 0.5f);

            // HUD: timer in the upper-left (spec §8), score below it. The upper-right stays free for the minimap.
            RectTransform hud = Panel(u, parent, "HUD", null, true);
            Text(u, hud, "TimeLabel", "Time Remaining", 30, topLeft, new Vector2(40f, -30f), new Vector2(400f, 40f), TextAlignmentOptions.TopLeft, font);
            TMP_Text timerText = Text(u, hud, "TimerText", "05:00", 72, topLeft, new Vector2(40f, -68f), new Vector2(400f, 80f), TextAlignmentOptions.TopLeft, font);
            TMP_Text scoreText = Text(u, hud, "ScoreText", "Score: 0", 36, topLeft, new Vector2(40f, -155f), new Vector2(400f, 50f), TextAlignmentOptions.TopLeft, font);

            // Intro (spec §1 wording) + controls (spec table).
            RectTransform intro = Panel(u, parent, "IntroPanel", overlay, true);
            Text(u, intro, "Title", "Puzzle Master", 110, center, new Vector2(0f, 300f), new Vector2(1400f, 140f), TextAlignmentOptions.Center, font);
            Text(u, intro, "Objectives",
                "Explore the region.\nRecover monsters.\nCollect puzzle items.\nSolve all five puzzles before time runs out.",
                40, center, new Vector2(0f, 90f), new Vector2(1400f, 240f), TextAlignmentOptions.Center, font);
            Text(u, intro, "Prompt", "Press SPACE to Start\nPress ESC to Quit", 46, center, new Vector2(0f, -120f), new Vector2(1400f, 130f), TextAlignmentOptions.Center, font);
            Text(u, intro, "Controls",
                "WASD  Move     Mouse  Look Around     Left Mouse Button  Attack     M  Toggle Minimap     ESC  Quit Game",
                28, center, new Vector2(0f, -330f), new Vector2(1700f, 50f), TextAlignmentOptions.Center, font);

            // Win (spec §10) and Lose (spec §11). Hidden while editing; GameScreens switches them at runtime.
            RectTransform win = Panel(u, parent, "WinPanel", overlay, false);
            Text(u, win, "Title", "Congratulations!", 100, center, new Vector2(0f, 230f), new Vector2(1400f, 130f), TextAlignmentOptions.Center, font);
            Text(u, win, "Message", "All puzzles completed.", 46, center, new Vector2(0f, 110f), new Vector2(1400f, 70f), TextAlignmentOptions.Center, font);
            TMP_Text winResult = Text(u, win, "Result", "Final Score: 0\nTime Left: 00:00", 38, center, new Vector2(0f, -10f), new Vector2(1400f, 110f), TextAlignmentOptions.Center, font);
            Text(u, win, "Prompt", "Press R to Restart\nPress ESC to Quit", 40, center, new Vector2(0f, -190f), new Vector2(1400f, 120f), TextAlignmentOptions.Center, font);

            RectTransform lose = Panel(u, parent, "LosePanel", overlay, false);
            Text(u, lose, "Title", "Time's Up!", 100, center, new Vector2(0f, 230f), new Vector2(1400f, 130f), TextAlignmentOptions.Center, font);
            Text(u, lose, "Message", "You Failed to Complete the Puzzles.", 46, center, new Vector2(0f, 110f), new Vector2(1400f, 70f), TextAlignmentOptions.Center, font);
            TMP_Text loseResult = Text(u, lose, "Result", "Final Score: 0", 38, center, new Vector2(0f, -10f), new Vector2(1400f, 60f), TextAlignmentOptions.Center, font);
            Text(u, lose, "Prompt", "Press R to Restart\nPress ESC to Quit", 40, center, new Vector2(0f, -190f), new Vector2(1400f, 120f), TextAlignmentOptions.Center, font);

            var hudController = u.GetOrAdd<HudController>(hud.gameObject);
            u.LinkIfEmpty(hudController, "timer", timer);
            u.LinkIfEmpty(hudController, "score", score);
            u.LinkIfEmpty(hudController, "timerText", timerText);
            u.LinkIfEmpty(hudController, "scoreText", scoreText);

            var screens = u.GetOrAdd<GameScreens>(root);
            u.LinkIfEmpty(screens, "gameState", gameState);
            u.LinkIfEmpty(screens, "score", score);
            u.LinkIfEmpty(screens, "timer", timer);
            u.LinkIfEmpty(screens, "introPanel", intro.gameObject);
            u.LinkIfEmpty(screens, "hudPanel", hud.gameObject);
            u.LinkIfEmpty(screens, "winPanel", win.gameObject);
            u.LinkIfEmpty(screens, "losePanel", lose.gameObject);
            u.LinkIfEmpty(screens, "winResultText", winResult);
            u.LinkIfEmpty(screens, "loseResultText", loseResult);
        }

        /// <summary>Finds or creates a full-screen panel (optionally with a background colour).</summary>
        internal static RectTransform Panel(SetupUtils u, Transform parent, string name, Color? background, bool activeByDefault)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return (RectTransform)existing;

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Setup");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;   // stretch to fill the canvas
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            if (background.HasValue)
            {
                var image = go.AddComponent<Image>();
                image.color = background.Value;
                image.raycastTarget = false;
            }
            go.SetActive(activeByDefault);
            u.Created($"UI panel {parent.name}/{name}");
            return rect;
        }

        /// <summary>
        /// Finds or creates a TextMeshPro label. The text, size and position are only set when it is
        /// created, so the PO can restyle it afterwards and a rerun keeps the changes.
        /// </summary>
        internal static TMP_Text Text(SetupUtils u, Transform parent, string name, string text, float size,
            Vector2 anchor, Vector2 position, Vector2 boxSize, TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                var found = existing.GetComponent<TMP_Text>();
                // Repair: a label created before the TMP essentials were imported has no font.
                if (found != null && found.font == null && font != null)
                {
                    found.font = font;
                    u.Linked($"font on {parent.name}/{name}");
                }
                return found;
            }

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Setup");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = boxSize;

            var label = go.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;   // labels never block clicks
            u.Created($"UI text {parent.name}/{name}");
            return label;
        }
    }
}
