using PuzzleMaster.Audio;
using PuzzleMaster.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 4 setup (ADR-0005; PB-25). Idempotent: safe to run again; it only creates what is
    /// missing and never overwrites references or values you already set.
    /// - Systems/Music: AudioSource + MusicPlayer, linked to the GameStateManager and the first
    ///   Audio/bgm_* clip (the PA1 Halloween track, ADR-0011). An existing MusicPlayer is reused
    ///   wherever it was moved in the hierarchy.
    /// Menu: Puzzle Master → Setup → Sprint 4 (Music)
    /// Batch: Unity -batchmode -quit -projectPath . -executeMethod PuzzleMaster.EditorTools.Sprint4Setup.RunBatch
    /// </summary>
    public static class Sprint4Setup
    {
        private const string AudioFolder = SetupUtils.ProjectRoot + "/Audio";
        private const string MusicPrefix = "bgm_";

        [MenuItem("Puzzle Master/Setup/Sprint 4 (Music)")]
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

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Sprint 4 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        /// <summary>The first AudioClip in _Project/Audio whose file name starts with "bgm_" (null if none).</summary>
        private static AudioClip FindMusic(SetupUtils u)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileName(path).StartsWith(MusicPrefix))
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            u.Warn($"no '{MusicPrefix}*' clip in {AudioFolder}");
            return null;
        }
    }
}
