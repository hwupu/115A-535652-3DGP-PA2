using System;
using PuzzleMaster.Audio;
using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using PuzzleMaster.Items;
using PuzzleMaster.Monsters;
using PuzzleMaster.Player;
using PuzzleMaster.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 2 setup (ADR-0005; PB-09 … PB-16). Idempotent: safe to run again; it only creates
    /// what is missing and never overwrites references or values you already set.
    /// - Item assets Key / Crystal / Artifact (Settings/Items) and their greybox pickup prefabs.
    /// - Greybox prefabs: SpellProjectile, and Monster (Health, Monster, SpellCaster,
    ///   MonsterSelfHeal, world-space LifeBar, CastPoint, Model).
    /// - Five monsters, one per Gameplay/MonsterSpots/MonsterSpot_n: drops Key, Crystal ×3, Artifact;
    ///   spots 1, 3, 5 patrol between two waypoints, 2 and 4 stand still (PO puzzle mapping, ADR-0009).
    /// - Player: Health (team Player), PlayerHitResponse, PlayerAttack + sword visual, Inventory.
    /// - Systems/AudioManager (placeholder tones), HUD inventory line.
    /// Menu: Puzzle Master → Setup → Sprint 2 (Monsters & Combat)
    /// Batch: Unity -batchmode -quit -projectPath . -executeMethod PuzzleMaster.EditorTools.Sprint2Setup.RunBatch
    /// </summary>
    public static class Sprint2Setup
    {
        public const string PrefabFolder = SetupUtils.ProjectRoot + "/Prefabs/Greybox";
        public const string ItemFolder = SetupUtils.ProjectRoot + "/Settings/Items";

        // Monster spots (from Sprint 1): which item each monster drops and whether it patrols.
        private static readonly (string item, MonsterMode mode)[] MonsterPlan =
        {
            ("Key", MonsterMode.Patrol),
            ("Crystal", MonsterMode.Stationary),
            ("Crystal", MonsterMode.Patrol),
            ("Crystal", MonsterMode.Stationary),
            ("Artifact", MonsterMode.Patrol),
        };

        [MenuItem("Puzzle Master/Setup/Sprint 2 (Monsters & Combat)")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Sprint 2 setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string log = Run();
            EditorUtility.DisplayDialog("Sprint 2 setup finished",
                "Done. See the Console for the full list of created / linked items.\n\nNext: press Play and follow the checklist in docs/guides/sprint-02-setup.md.", "OK");
            Debug.Log(log);
        }

        public static void RunBatch() => Debug.Log(Run());

        private static string Run()
        {
            var u = new SetupUtils();

            // Open the scene first: OpenScene unloads unused assets, which would invalidate
            // references to prefabs created earlier in this run (a PA1 lesson).
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != Sprint0Setup.MainScenePath)
                scene = EditorSceneManager.OpenScene(Sprint0Setup.MainScenePath, OpenSceneMode.Single);

            // ---------- Materials ----------
            Material mMonster = u.GetOrCreateMaterial("M_Monster", new Color(0.45f, 0.25f, 0.6f));
            Material mMonsterHat = u.GetOrCreateMaterial("M_MonsterHat", new Color(0.18f, 0.1f, 0.28f));
            Material mMonsterEye = u.GetOrCreateMaterial("M_MonsterEye", new Color(1f, 0.9f, 0.2f));
            Material mSpell = u.GetOrCreateMaterial("M_Spell", new Color(0.8f, 0.4f, 1f));
            Material mKey = u.GetOrCreateMaterial("M_Key", new Color(1f, 0.8f, 0.15f));
            Material mCrystal = u.GetOrCreateMaterial("M_Crystal", new Color(0.3f, 0.85f, 1f));
            Material mArtifact = u.GetOrCreateMaterial("M_Artifact", new Color(1f, 0.5f, 0.15f));
            Material mSword = u.GetOrCreateMaterial("M_Sword", new Color(0.8f, 0.8f, 0.85f));

            // ---------- Items + pickups (PB-14) ----------
            u.EnsureFolder(PrefabFolder);
            u.EnsureFolder(ItemFolder);
            ItemDefinition key = GetOrCreateItem(u, "Key", mKey.GetColor("_BaseColor"), mKey, (model, m) =>
            {
                SetupUtils.Visual(PrimitiveType.Cube, model, "Bow", new Vector3(0f, 0.2f, 0f), new Vector3(0.3f, 0.3f, 0.08f), m);
                SetupUtils.Visual(PrimitiveType.Cube, model, "Shaft", new Vector3(0f, -0.15f, 0f), new Vector3(0.08f, 0.45f, 0.08f), m);
                SetupUtils.Visual(PrimitiveType.Cube, model, "Tooth", new Vector3(0.08f, -0.32f, 0f), new Vector3(0.12f, 0.06f, 0.08f), m);
            });
            ItemDefinition crystal = GetOrCreateItem(u, "Crystal", mCrystal.GetColor("_BaseColor"), mCrystal, (model, m) =>
                SetupUtils.Visual(PrimitiveType.Cube, model, "Gem", Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), m, new Vector3(45f, 0f, 45f)));
            ItemDefinition artifact = GetOrCreateItem(u, "Artifact", mArtifact.GetColor("_BaseColor"), mArtifact, (model, m) =>
            {
                SetupUtils.Visual(PrimitiveType.Cylinder, model, "Base", new Vector3(0f, -0.15f, 0f), new Vector3(0.4f, 0.08f, 0.4f), m);
                SetupUtils.Visual(PrimitiveType.Sphere, model, "Orb", new Vector3(0f, 0.1f, 0f), new Vector3(0.3f, 0.3f, 0.3f), m);
            });
            ItemDefinition ItemByName(string itemName) =>
                itemName == "Key" ? key : itemName == "Crystal" ? crystal : artifact;

            // ---------- Spell + Monster prefabs (PB-09 … PB-13) ----------
            SpellProjectile spellPrefab = GetOrCreatePrefab(u, "SpellProjectile", root =>
            {
                var collider = root.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.25f;
                var body = root.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                root.AddComponent<SpellProjectile>();
                SetupUtils.Visual(PrimitiveType.Sphere, root.transform, "Model", Vector3.zero, Vector3.one * 0.4f, mSpell);
            }).GetComponent<SpellProjectile>();

            Monster monsterPrefab = GetOrCreatePrefab(u, "Monster", root =>
                BuildMonster(root, spellPrefab, mMonster, mMonsterHat, mMonsterEye)).GetComponent<Monster>();

            // ---------- Monsters in the scene ----------
            for (int i = 0; i < MonsterPlan.Length; i++)
                PlaceMonster(u, scene, i + 1, monsterPrefab, ItemByName(MonsterPlan[i].item), MonsterPlan[i].mode);

            // ---------- Player (PB-10, PB-11, PB-14) ----------
            GameObject manager = u.FindOrCreate(scene, "Systems/GameManager");
            var gameInput = manager.GetComponent<GameInput>();
            if (gameInput == null) u.Warn("Systems/GameManager has no GameInput: run Setup → Sprint 1 first");
            Inventory inventory = SetupPlayer(u, scene, gameInput, mSword);

            // ---------- Audio (PB-16) ----------
            GameObject audio = u.FindOrCreate(scene, "Systems/AudioManager");
            u.GetOrAdd<AudioSource>(audio);
            u.GetOrAdd<AudioManager>(audio);

            // ---------- HUD inventory line ----------
            Transform hud = SetupUtils.FindRoot(scene, "UI")?.Find("Screens/HUD");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Sprint1Setup.TmpFontPath);
            if (hud == null || font == null) u.Warn("UI/Screens/HUD or the TMP font is missing: run Setup → Sprint 1 first");
            else
            {
                TMP_Text text = Sprint1Setup.Text(u, hud, "InventoryText", "Items: none", 30, new Vector2(0f, 1f),
                    new Vector2(40f, -205f), new Vector2(900f, 45f), TextAlignmentOptions.TopLeft, font);
                var inventoryHud = u.GetOrAdd<InventoryHud>(text.gameObject);
                u.LinkIfEmpty(inventoryHud, "inventory", inventory);
                u.LinkIfEmpty(inventoryHud, "text", text);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Sprint 2 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        // ================================================================== Assets

        /// <summary>
        /// Loads _Project/Prefabs/Greybox/{name}.prefab, or builds it: <paramref name="build"/> fills a
        /// temporary root object (pivot at the ground contact), which is saved as a prefab and deleted.
        /// An existing prefab is never rebuilt, so the PO's model swaps are kept.
        /// </summary>
        private static GameObject GetOrCreatePrefab(SetupUtils u, string name, Action<GameObject> build)
        {
            string path = $"{PrefabFolder}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                u.Skipped(path);
                return existing;
            }
            var root = new GameObject(name);
            try
            {
                build(root);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                u.Created(path);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);   // the temporary object must never stay in the scene
            }
        }

        /// <summary>Item asset (Settings/Items/Item_{name}) + its pickup prefab (Prefabs/Greybox/Pickup_{name}), linked both ways.</summary>
        private static ItemDefinition GetOrCreateItem(SetupUtils u, string itemName, Color color, Material material,
            Action<Transform, Material> buildModel)
        {
            string path = $"{ItemFolder}/Item_{itemName}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null) u.Skipped(path);
            else
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
                SetupUtils.SetValue(item, "displayName", p => p.stringValue = itemName);
                SetupUtils.SetValue(item, "color", p => p.colorValue = color);
                u.Created(path);
            }

            // Pickup: trigger + kinematic Rigidbody + PuzzleItem on the root, visuals in a bobbing Model child.
            ItemDefinition captured = item;
            PuzzleItem pickup = GetOrCreatePrefab(u, $"Pickup_{itemName}", root =>
            {
                var collider = root.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.8f;
                collider.center = new Vector3(0f, 0.6f, 0f);
                var body = root.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                var puzzleItem = root.AddComponent<PuzzleItem>();
                var model = new GameObject("Model").transform;
                model.SetParent(root.transform, false);
                model.localPosition = new Vector3(0f, 0.6f, 0f);
                buildModel(model, material);
                SetupUtils.SetValue(puzzleItem, "item", p => p.objectReferenceValue = captured);
                SetupUtils.SetValue(puzzleItem, "visual", p => p.objectReferenceValue = model);
            }).GetComponent<PuzzleItem>();

            u.LinkIfEmpty(item, "pickupPrefab", pickup);
            return item;
        }

        /// <summary>Greybox monster: capsule body, hat, eyes (show its facing), cast point, life bar.</summary>
        private static void BuildMonster(GameObject root, SpellProjectile spellPrefab, Material body, Material hat, Material eye)
        {
            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.height = 1.8f;
            collider.radius = 0.45f;
            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;   // moved by code (patrol), pushed by nothing
            rigidbody.useGravity = false;

            var health = root.AddComponent<Health>();   // team defaults to Monster
            var monster = root.AddComponent<Monster>();
            var caster = root.AddComponent<SpellCaster>();
            root.AddComponent<MonsterSelfHeal>();

            var model = new GameObject("Model").transform;
            model.SetParent(root.transform, false);
            SetupUtils.Visual(PrimitiveType.Capsule, model, "Body", new Vector3(0f, 0.9f, 0f), new Vector3(0.9f, 0.9f, 0.9f), body);
            SetupUtils.Visual(PrimitiveType.Cylinder, model, "HatBrim", new Vector3(0f, 1.85f, 0f), new Vector3(0.9f, 0.04f, 0.9f), hat);
            SetupUtils.Visual(PrimitiveType.Cube, model, "HatTop", new Vector3(0f, 2.1f, 0f), new Vector3(0.4f, 0.45f, 0.4f), hat, new Vector3(0f, 45f, 0f));
            SetupUtils.Visual(PrimitiveType.Cube, model, "Eye_L", new Vector3(-0.15f, 1.45f, 0.4f), Vector3.one * 0.12f, eye);
            SetupUtils.Visual(PrimitiveType.Cube, model, "Eye_R", new Vector3(0.15f, 1.45f, 0.4f), Vector3.one * 0.12f, eye);

            var castPoint = new GameObject("CastPoint").transform;
            castPoint.SetParent(root.transform, false);
            castPoint.localPosition = new Vector3(0f, 1.3f, 0.6f);

            BuildLifeBar(root.transform, health);

            SetupUtils.SetValue(monster, "caster", p => p.objectReferenceValue = caster);
            SetupUtils.SetValue(monster, "model", p => p.objectReferenceValue = model);
            SetupUtils.SetValue(caster, "spellPrefab", p => p.objectReferenceValue = spellPrefab);
            SetupUtils.SetValue(caster, "castPoint", p => p.objectReferenceValue = castPoint);
        }

        /// <summary>
        /// World-space canvas above the head: 1.2 m × 0.14 m (120 × 14 units at scale 0.01).
        /// The Fill's pivot is at its left edge, so scaling x shrinks it towards the left.
        /// </summary>
        private static LifeBar BuildLifeBar(Transform parent, Health health)
        {
            var go = new GameObject("LifeBar", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.localPosition = new Vector3(0f, 2.65f, 0f);
            rect.sizeDelta = new Vector2(120f, 14f);
            rect.localScale = Vector3.one * 0.01f;
            go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;

            Image background = UiImage(rect, "Background", new Color(0f, 0f, 0f, 0.6f), new Vector2(0.5f, 0.5f));
            Image fill = UiImage(rect, "Fill", new Color(0.3f, 0.9f, 0.3f), new Vector2(0f, 0.5f));
            ((RectTransform)fill.transform).offsetMin = new Vector2(2f, 2f);   // 2-unit border inside the background
            ((RectTransform)fill.transform).offsetMax = new Vector2(-2f, -2f);
            background.raycastTarget = false;

            var lifeBar = go.AddComponent<LifeBar>();
            SetupUtils.SetValue(lifeBar, "health", p => p.objectReferenceValue = health);
            SetupUtils.SetValue(lifeBar, "fill", p => p.objectReferenceValue = fill.rectTransform);
            SetupUtils.SetValue(lifeBar, "fillImage", p => p.objectReferenceValue = fill);
            return lifeBar;
        }

        /// <summary>A stretched UI Image child with the given pivot.</summary>
        private static Image UiImage(RectTransform parent, string name, Color color, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = pivot;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // ================================================================== Scene

        /// <summary>
        /// Puts a Monster prefab instance at MonsterSpot_n (once) and sets its drop, mode and
        /// waypoints as instance overrides. An existing monster under the spot is left as is.
        /// </summary>
        private static void PlaceMonster(SetupUtils u, Scene scene, int index, Monster prefab, ItemDefinition drop, MonsterMode mode)
        {
            Transform spot = u.FindOrCreate(scene, $"Gameplay/MonsterSpots/MonsterSpot_{index}").transform;
            if (spot.GetComponentInChildren<Monster>(true) != null)
            {
                u.Skipped($"monster at MonsterSpot_{index}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Setup");
            instance.name = $"Monster_{index}_{drop.DisplayName}";
            instance.transform.SetParent(spot, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // face south, towards the player start

            var monster = instance.GetComponent<Monster>();
            var so = new SerializedObject(monster);
            so.FindProperty("drop").objectReferenceValue = drop;
            so.FindProperty("mode").enumValueIndex = (int)mode;
            if (mode == MonsterMode.Patrol)
            {
                // Waypoints are siblings under the spot (not children of the moving monster).
                Transform a = u.FindOrCreate(scene, $"Gameplay/MonsterSpots/MonsterSpot_{index}/Waypoint_A").transform;
                Transform b = u.FindOrCreate(scene, $"Gameplay/MonsterSpots/MonsterSpot_{index}/Waypoint_B").transform;
                a.localPosition = new Vector3(-4f, 0f, 0f);
                b.localPosition = new Vector3(4f, 0f, 0f);
                SerializedProperty points = so.FindProperty("waypoints");
                points.arraySize = 2;
                points.GetArrayElementAtIndex(0).objectReferenceValue = a;
                points.GetArrayElementAtIndex(1).objectReferenceValue = b;
            }
            so.ApplyModifiedProperties();   // stored as prefab instance overrides
            u.Created($"{instance.name} ({mode}, drops {drop.DisplayName}) at MonsterSpot_{index}");
        }

        private static Inventory SetupPlayer(SetupUtils u, Scene scene, GameInput gameInput, Material sword)
        {
            GameObject player = u.FindOrCreate(scene, "Gameplay/Player");

            bool newHealth = !player.TryGetComponent(out Health _);
            var health = u.GetOrAdd<Health>(player);
            if (newHealth) SetupUtils.SetValue(health, "team", p => p.enumValueIndex = (int)Team.Player);

            var hitResponse = u.GetOrAdd<PlayerHitResponse>(player);
            u.LinkIfEmpty(hitResponse, "motor", player.GetComponent<PlayerMotor>());

            var inventory = u.GetOrAdd<Inventory>(player);

            // Sword visual on a pivot at chest height; PlayerAttack sweeps the pivot on each swing.
            Transform model = u.FindOrCreate(scene, "Gameplay/Player/Model").transform;
            Transform pivot = u.FindOrCreate(scene, "Gameplay/Player/Model/SwordPivot").transform;
            if (pivot.childCount == 0) pivot.localPosition = new Vector3(0f, 1.1f, 0f);
            u.FindOrCreatePrimitive(pivot, "Sword", PrimitiveType.Cube, new Vector3(0.45f, 0f, 0.55f),
                new Vector3(0.08f, 0.08f, 0.9f), sword, keepCollider: false);

            var attack = u.GetOrAdd<PlayerAttack>(player);
            u.LinkIfEmpty(attack, "input", gameInput);
            u.LinkIfEmpty(attack, "swingPivot", pivot);

            // New children (the sword) must be on the Player layer too (the camera ignores that layer).
            int layer = player.layer;
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.layer == layer) continue;
                SetupUtils.SetLayerRecursively(player, layer);
                u.Linked("Player children → Player layer");
                break;
            }
            return inventory;
        }
    }
}
