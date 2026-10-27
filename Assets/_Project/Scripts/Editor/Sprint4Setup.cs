using System.IO;
using PuzzleMaster.Audio;
using PuzzleMaster.Core;
using PuzzleMaster.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 4 setup (ADR-0005; PB-25, PB-27). Idempotent: safe to run again; it only creates what is
    /// missing and never overwrites references or values you already set.
    /// - Systems/Music: AudioSource + MusicPlayer, linked to the GameStateManager and the first
    ///   Audio/bgm_* clip (the PA1 Halloween track, ADR-0011). An existing MusicPlayer is reused
    ///   wherever it was moved in the hierarchy.
    /// - Input: adds the Gameplay action OrbitHold (middle mouse / Left Alt) to PlayerControls if it is
    ///   missing; the CameraRig orbits around the player while it is held (demo request, PA1 feature).
    /// - Story (PB-27, the spec's Hero's Journey): Systems/Story (StoryBeats: start + recovery lines),
    ///   a prologue above the intro title and an epilogue on the win / lose screens. The spec's
    ///   required texts are not changed; the story lines are extra texts the PO can edit or move.
    /// Menu: Puzzle Master → Setup → Sprint 4 (Music, Story & Orbit Camera)
    /// Batch: Unity -batchmode -quit -projectPath . -executeMethod PuzzleMaster.EditorTools.Sprint4Setup.RunBatch
    /// </summary>
    public static class Sprint4Setup
    {
        private const string AudioFolder = SetupUtils.ProjectRoot + "/Audio";
        private const string MusicPrefix = "bgm_";

        // Story texts (PB-27), condensed from the spec's "The Story" section.
        private const string Prologue =
            "Dark magic has shattered the five Crystal Seals of Lumina and corrupted their guardians.\n"
            + "You alone dare to enter the Forgotten Region.";
        private const string WinEpilogue =
            "The Crystal Seals are restored and the dark magic is cleansed.\n"
            + "The guardians regain their true forms. Peace returns to the Forgotten Region.";
        private const string LoseEpilogue =
            "The corruption has consumed the Forgotten Region.\n"
            + "The seals stay broken... but a hero can always try again.";
        private static readonly Color StoryColor = new Color(1f, 0.85f, 0.55f);   // warm parchment, apart from the white spec texts

        [MenuItem("Puzzle Master/Setup/Sprint 4 (Music, Story & Orbit Camera)")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Sprint 4 setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string log = Run();
            EditorUtility.DisplayDialog("Sprint 4 setup finished",
                "Done. See the Console for the full list of created / linked items.\n\nNext: press Play and follow the checklist in docs/guides/sprint-04-setup.md.", "OK");
            Debug.Log(log);
        }

        public static void RunBatch() => Debug.Log(Run());

        private static string Run()
        {
            var u = new SetupUtils();

            // Open the scene first: OpenScene unloads in-memory assets created earlier in a run.
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != Sprint0Setup.MainScenePath)
                scene = EditorSceneManager.OpenScene(Sprint0Setup.MainScenePath, OpenSceneMode.Single);

            // ---------- Background music (PB-25) ----------
            var gameState = Object.FindAnyObjectByType<GameStateManager>(FindObjectsInactive.Include);
            if (gameState == null) u.Warn("no GameStateManager in the scene: run Setup → Sprint 1 first");

            // Found by component, not by path: the PO may have moved it.
            var player = Object.FindAnyObjectByType<MusicPlayer>(FindObjectsInactive.Include);
            GameObject go = player != null ? player.gameObject : u.FindOrCreate(scene, "Systems/Music");
            u.GetOrAdd<AudioSource>(go);
            player = u.GetOrAdd<MusicPlayer>(go);
            u.LinkIfEmpty(player, "gameState", gameState);
            u.LinkIfEmpty(player, "music", FindMusic(u));

            // ---------- Story (PB-27) ----------
            var story = Object.FindAnyObjectByType<StoryBeats>(FindObjectsInactive.Include);
            GameObject storyGo = story != null ? story.gameObject : u.FindOrCreate(scene, "Systems/Story");
            story = u.GetOrAdd<StoryBeats>(storyGo);
            u.LinkIfEmpty(story, "gameState", gameState);
            AddStoryTexts(u, scene);

            // ---------- Orbit camera input ----------
            EnsureOrbitHold(u);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Sprint 4 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        /// <summary>
        /// Adds a "Story" text to the intro (above the title) and the win / lose panels (under the
        /// prompts). Existing Story texts are left alone, so the PO's edits and moves are kept.
        /// </summary>
        private static void AddStoryTexts(SetupUtils u, Scene scene)
        {
            Transform screens = SetupUtils.FindRoot(scene, "UI")?.Find("Screens");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Sprint1Setup.TmpFontPath);
            if (screens == null || font == null)
            {
                u.Warn("UI/Screens or the TMP font is missing: run Setup → Sprint 1 first");
                return;
            }
            var center = new Vector2(0.5f, 0.5f);
            StoryText(u, screens, "IntroPanel", Prologue, 30, new Vector2(0f, 425f), font, center);   // above the title, like an opening crawl
            StoryText(u, screens, "WinPanel", WinEpilogue, 32, new Vector2(0f, -340f), font, center);
            StoryText(u, screens, "LosePanel", LoseEpilogue, 32, new Vector2(0f, -340f), font, center);
        }

        private static void StoryText(SetupUtils u, Transform screens, string panelName, string text, float size,
            Vector2 position, TMP_FontAsset font, Vector2 anchor)
        {
            Transform panel = screens.Find(panelName);
            if (panel == null)
            {
                u.Warn($"UI/Screens/{panelName} not found: run Setup → Sprint 1 first");
                return;
            }
            if (panel.Find("Story") != null)
            {
                u.Skipped($"{panelName}/Story");
                return;
            }
            TMP_Text label = Sprint1Setup.Text(u, panel, "Story", text, size, anchor, position,
                new Vector2(1700f, 90f), TextAlignmentOptions.Center, font);
            label.fontStyle = FontStyles.Italic;
            label.color = StoryColor;
        }

        /// <summary>Adds OrbitHold to the Gameplay map; used here and by Sprint 1 for a newly generated asset.</summary>
        public static void AddOrbitHold(InputActionMap gameplay)
        {
            InputAction orbit = gameplay.AddAction("OrbitHold", InputActionType.Button, "<Mouse>/middleButton");
            orbit.AddBinding("<Keyboard>/leftAlt");   // for trackpads / mice without a middle button
        }

        /// <summary>
        /// Adds OrbitHold to the existing PlayerControls.inputactions if it is missing. The asset is a
        /// JSON file: read it, add the action in memory, write it back, reimport. Other bindings are kept.
        /// </summary>
        private static void EnsureOrbitHold(SetupUtils u)
        {
            string path = Sprint1Setup.InputActionsPath;
            if (!File.Exists(path))
            {
                u.Warn($"{path} is missing: run Setup → Sprint 1 first");
                return;
            }
            InputActionAsset asset = InputActionAsset.FromJson(File.ReadAllText(path));
            try
            {
                InputActionMap gameplay = asset.FindActionMap(GameInput.GameplayMap, true);
                if (gameplay.FindAction("OrbitHold") != null)
                {
                    u.Skipped("input action Gameplay/OrbitHold");
                    return;
                }
                AddOrbitHold(gameplay);
                File.WriteAllText(path, asset.ToJson());
                AssetDatabase.ImportAsset(path);
                u.Created("input action Gameplay/OrbitHold (middle mouse, Left Alt)");
            }
            finally
            {
                Object.DestroyImmediate(asset);   // FromJson makes an in-memory object; don't leak it
            }
        }

        /// <summary>The first AudioClip in _Project/Audio whose file name starts with "bgm_" (null if none).</summary>
        private static AudioClip FindMusic(SetupUtils u)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path).StartsWith(MusicPrefix))
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            u.Warn($"no '{MusicPrefix}*' clip in {AudioFolder}");
            return null;
        }
    }
}
