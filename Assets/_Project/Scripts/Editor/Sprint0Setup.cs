using System.Linq;
using PuzzleMaster.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 0 setup (ADR-0005, PB-00). Idempotent: safe to run again; it only creates or
    /// changes what is missing.
    /// - Player Settings: Product Name "Puzzle Master", Company Name "NYCU 3DGP".
    /// - The _Project folder skeleton (Scenes, Prefabs, Materials, Models, Audio, Settings).
    /// - Assets/_Project/Scenes/Main.unity with the hierarchy roots Environment / Gameplay /
    ///   Cameras / Systems / UI, the default camera and light moved under them, and
    ///   Systems/GameManager with a ConfigLoader (ADR-0004).
    /// - Build Profiles scene list = Main only.
    /// - Removes the URP template's tutorial content (TutorialInfo, Readme, SampleScene).
    /// Menu: Puzzle Master → Setup → Sprint 0 (Project Skeleton)
    /// Batch: Unity -batchmode -quit -projectPath . -executeMethod PuzzleMaster.EditorTools.Sprint0Setup.RunBatch
    /// </summary>
    public static class Sprint0Setup
    {
        public const string MainScenePath = SetupUtils.ProjectRoot + "/Scenes/Main.unity";
        private const string ProductName = "Puzzle Master";
        private const string CompanyName = "NYCU 3DGP";

        private static readonly string[] Folders = { "Scenes", "Prefabs", "Materials", "Models", "Audio", "Settings" };
        private static readonly string[] SceneRoots = { "Environment", "Gameplay", "Cameras", "Systems", "UI" };
        // Template content we don't use. Deleted through AssetDatabase so the .meta files go too.
        private static readonly string[] TemplateLeftovers =
        {
            "Assets/Readme.asset", "Assets/TutorialInfo", "Assets/Scenes/SampleScene.unity",
        };
        // Template folder removed only once it is empty (never delete something the PO put there).
        private const string TemplateScenesFolder = "Assets/Scenes";

        /// <summary>Menu entry: refuses to run in Play Mode and lets the user save open scene changes first.</summary>
        [MenuItem("Puzzle Master/Setup/Sprint 0 (Project Skeleton)")]
        public static void RunFromMenu()
        {
            // Edits made in Play Mode are thrown away when Play stops, so setup only runs in Edit Mode.
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Sprint 0 setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string log = Run();
            EditorUtility.DisplayDialog("Sprint 0 setup finished",
                "Done. See the Console for the full list of created / linked items.\n\nNext: follow docs/guides/sprint-00-setup.md.", "OK");
            Debug.Log(log);
        }

        /// <summary>Entry point for -executeMethod (no dialogs, so it works in -batchmode).</summary>
        public static void RunBatch() => Debug.Log(Run());

        /// <summary>Runs all steps and returns the text report.</summary>
        private static string Run()
        {
            var u = new SetupUtils();

            // ---------- Player Settings ----------
            if (PlayerSettings.productName != ProductName)
            {
                PlayerSettings.productName = ProductName;
                u.Linked($"Player Settings → Product Name = {ProductName}");
            }
            else u.Skipped($"Product Name {ProductName}");
            if (PlayerSettings.companyName != CompanyName)
            {
                PlayerSettings.companyName = CompanyName;
                u.Linked($"Player Settings → Company Name = {CompanyName}");
            }
            else u.Skipped($"Company Name {CompanyName}");

            // ---------- Folders ----------
            foreach (string folder in Folders) u.EnsureFolder($"{SetupUtils.ProjectRoot}/{folder}");

            // ---------- Main scene ----------
            // Open (or create) Main first: it may replace SampleScene as the open scene, which must
            // happen before SampleScene is deleted below.
            Scene scene = OpenOrCreateMainScene(u);

            foreach (string root in SceneRoots) u.FindOrCreate(scene, root);
            // The default new scene has "Main Camera" and "Directional Light" at the root; group them.
            Reparent(u, scene, "Main Camera", "Cameras");
            Reparent(u, scene, "Directional Light", "Environment");

            GameObject gameManager = u.FindOrCreate(scene, "Systems/GameManager");
            u.GetOrAdd<ConfigLoader>(gameManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // ---------- Build Profiles ----------
            if (EditorBuildSettings.scenes.Length == 1 && EditorBuildSettings.scenes[0].path == MainScenePath)
                u.Skipped("Build Profiles scene list = Main");
            else
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
                u.Linked("Build Profiles scene list = Main only");
            }

            // ---------- Template cleanup ----------
            foreach (string path in TemplateLeftovers) Delete(u, path);
            if (AssetDatabase.IsValidFolder(TemplateScenesFolder) && !AssetDatabase.FindAssets("", new[] { TemplateScenesFolder }).Any())
                Delete(u, TemplateScenesFolder);

            AssetDatabase.SaveAssets();
            return "Sprint 0 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        /// <summary>Opens Main.unity, creating it from Unity's default new scene (camera + light) if missing.</summary>
        private static Scene OpenOrCreateMainScene(SetupUtils u)
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.path == MainScenePath)
            {
                u.Skipped(MainScenePath);
                return active;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) != null)
            {
                u.Skipped(MainScenePath);
                return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, MainScenePath);
            u.Created(MainScenePath);
            return scene;
        }

        /// <summary>Deletes an asset or folder (with its .meta) if it still exists; logs it.</summary>
        private static void Delete(SetupUtils u, string path)
        {
            // A GUID lookup, not LoadMainAssetAtPath: Readme.asset can't be loaded once its script
            // (in TutorialInfo) is gone, but it still exists and must still be deleted.
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets))) return;
            if (AssetDatabase.DeleteAsset(path)) u.Removed(path);
            else u.Warn($"Could not delete {path}");
        }

        /// <summary>Moves a top-level object under a root group (once); logs it.</summary>
        private static void Reparent(SetupUtils u, Scene scene, string objectName, string parentName)
        {
            Transform child = SetupUtils.FindRoot(scene, objectName);
            if (child == null) return;   // already moved, or the PO removed it
            Transform parent = SetupUtils.FindRoot(scene, parentName);
            Undo.SetTransformParent(child, parent, "Setup");
            u.Linked($"{objectName} → {parentName}/");
        }
    }
}
