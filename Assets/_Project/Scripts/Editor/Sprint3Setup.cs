using PuzzleMaster.Cameras;
using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using PuzzleMaster.Items;
using PuzzleMaster.Puzzles;
using PuzzleMaster.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Sprint 3 setup (ADR-0005; PB-17 … PB-24). Idempotent: safe to run again; it only creates
    /// what is missing and never overwrites references or values you already set.
    /// - Puzzles on the five sites (Gameplay/PuzzleSites, wherever the PO moved them), greybox:
    ///   1 Locked Door (Key), 2 Crystal Altar (3 Crystals), 3 Twin Switches (2 pressure switches),
    ///   4 Artifact Pedestal (Artifact), 5 Final Gate (needs 1–4). Each site gets a trigger zone
    ///   (the blue disc), a status orb, a floating label and a minimap icon.
    /// - Systems/PuzzleManager (score +200, Win when all are solved); HUD objective list and message line.
    /// - Minimap (ADR-0010, ported from PA1): "Minimap" layer, icons on the player (arrow), monsters,
    ///   pickups, puzzles, ground and walls; an orthographic MinimapCamera → RT_Minimap shown in the
    ///   HUD's upper-right panel; M toggles it (MinimapToggle).
    /// Menu: Puzzle Master → Setup → Sprint 3 (Puzzles & Minimap)
    /// Batch: -executeMethod PuzzleMaster.EditorTools.Sprint3Setup.RunBatch
    /// </summary>
    public static class Sprint3Setup
    {
        private const string SitesPath = "Gameplay/PuzzleSites";
        private const string RenderTexturePath = SetupUtils.ProjectRoot + "/Settings/RT_Minimap.renderTexture";
        private const string ArrowMeshPath = SetupUtils.ProjectRoot + "/Settings/MinimapArrow.asset";

        // Icon heights: higher draws on top (ground < walls < puzzles < items < monsters < player).
        private const float GroundIconY = 10f, WallIconY = 11f, PuzzleIconY = 12f, ItemIconY = 13f, MonsterIconY = 14f, PlayerIconY = 15f;

        private static readonly Color PlayerColor = new Color(0.3f, 0.85f, 1f);
        private static readonly Color PuzzleColor = new Color(1f, 0.7f, 0.1f);   // = Puzzle.unsolvedColor
        private static readonly Color SolvedColor = new Color(0.25f, 1f, 0.35f);  // = Puzzle.solvedColor
        private static readonly Color MonsterColor = new Color(0.95f, 0.25f, 0.25f);
        private static readonly Color RecoveredColor = new Color(1f, 0.95f, 0.75f);

        [MenuItem("Puzzle Master/Setup/Sprint 3 (Puzzles & Minimap)")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Sprint 3 setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string log = Run();
            EditorUtility.DisplayDialog("Sprint 3 setup finished",
                "Done. See the Console for the full list of created / linked items.\n\nNext: press Play and follow the checklist in docs/guides/sprint-03-setup.md.", "OK");
            Debug.Log(log);
        }

        public static void RunBatch() => Debug.Log(Run());

        private static string Run()
        {
            var u = new SetupUtils();

            // Open the scene first: OpenScene unloads unused assets, which would invalidate
            // references to assets created earlier in this run (a PA1 lesson).
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != Sprint0Setup.MainScenePath)
                scene = EditorSceneManager.OpenScene(Sprint0Setup.MainScenePath, OpenSceneMode.Single);

            int layer = u.EnsureLayer(MinimapIcon.LayerName);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Sprint1Setup.TmpFontPath);
            var key = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{Sprint2Setup.ItemFolder}/Item_Key.asset");
            var crystal = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{Sprint2Setup.ItemFolder}/Item_Crystal.asset");
            var artifact = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{Sprint2Setup.ItemFolder}/Item_Artifact.asset");
            Transform sites = SetupUtils.FindRoot(scene, "Gameplay")?.Find("PuzzleSites");
            GameObject managerGo = SetupUtils.FindRoot(scene, "Systems")?.Find("GameManager")?.gameObject;
            if (layer < 0 || font == null || key == null || crystal == null || artifact == null || sites == null || managerGo == null)
            {
                u.Warn("Missing the Minimap layer, TMP font, item assets, Gameplay/PuzzleSites or Systems/GameManager: run Setups 1 and 2 first");
                return Finish(u, scene);
            }

            // ---------- Materials ----------
            Material mStone = u.GetOrCreateMaterial("M_PuzzleStone", new Color(0.42f, 0.4f, 0.38f));
            Material mWood = u.GetOrCreateMaterial("M_PuzzleDoor", new Color(0.35f, 0.22f, 0.12f));
            Material mIron = u.GetOrCreateMaterial("M_PuzzleIron", new Color(0.15f, 0.15f, 0.18f));
            Material mStatus = EnsureEmissive(u, u.GetOrCreateMaterial("M_PuzzleStatus", Color.white));
            Material mPad = EnsureEmissive(u, u.GetOrCreateMaterial("M_SwitchPad", Color.white));
            Material mIcon = u.GetOrCreateUnlitMaterial("MI_Icon", Color.white);   // tinted per icon (MinimapIcon.color)

            // ---------- Puzzles (PB-17 … PB-22) ----------
            var parts = new Parts(u, layer, font, mStone, mWood, mIron, mStatus, mPad, mIcon);
            Puzzle door = BuildDoor(parts, Site(u, sites, "PuzzleSite_1_Key"), key);
            Puzzle altar = BuildAltar(parts, Site(u, sites, "PuzzleSite_2_Crystals"), crystal);
            Puzzle switches = BuildSwitches(parts, Site(u, sites, "PuzzleSite_3_Switches"));
            Puzzle pedestal = BuildPedestal(parts, Site(u, sites, "PuzzleSite_4_Artifact"), artifact);
            Puzzle gate = BuildGate(parts, Site(u, sites, "PuzzleSite_5_FinalGate"), new[] { door, altar, switches, pedestal });
            Puzzle[] all = { door, altar, switches, pedestal, gate };

            GameObject managerObject = u.FindOrCreate(scene, "Systems/PuzzleManager");
            var manager = u.GetOrAdd<PuzzleManager>(managerObject);
            u.LinkIfEmpty(manager, "gameState", managerGo.GetComponent<GameStateManager>());
            u.LinkIfEmpty(manager, "score", managerGo.GetComponent<ScoreManager>());
            LinkArrayIfEmpty(u, manager, "puzzles", all);

            // ---------- HUD: objective list + message line ----------
            Transform hud = SetupUtils.FindRoot(scene, "UI")?.Find("Screens/HUD");
            if (hud == null) u.Warn("UI/Screens/HUD not found: run Setup → Sprint 1 first");
            else
            {
                TMP_Text list = Sprint1Setup.Text(u, hud, "PuzzleText", "Puzzles 0/5", 28, new Vector2(0f, 1f),
                    new Vector2(40f, -260f), new Vector2(760f, 260f), TextAlignmentOptions.TopLeft, font);
                var puzzleHud = u.GetOrAdd<PuzzleHud>(list.gameObject);
                u.LinkIfEmpty(puzzleHud, "manager", manager);
                u.LinkIfEmpty(puzzleHud, "text", list);

                TMP_Text message = Sprint1Setup.Text(u, hud, "MessageText", "", 38, new Vector2(0.5f, 0f),
                    new Vector2(0f, 150f), new Vector2(1600f, 60f), TextAlignmentOptions.Center, font);
                var hudMessage = u.GetOrAdd<HudMessage>(message.gameObject);
                u.LinkIfEmpty(hudMessage, "text", message);
            }

            // ---------- Minimap (PB-23, PB-24) ----------
            SetupMinimap(u, scene, layer, mIcon, hud, managerGo.GetComponent<GameInput>(), key, crystal, artifact, font);

            return Finish(u, scene);
        }

        private static string Finish(SetupUtils u, Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Sprint 3 setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }

        private static Transform Site(SetupUtils u, Transform sites, string name)
        {
            Transform site = sites.Find(name);
            if (site == null) u.Warn($"{SitesPath}/{name} not found (renamed?): its puzzle is skipped");
            return site;
        }

        /// <summary>Turns on emission (white) so a MaterialPropertyBlock can make it glow in any colour.</summary>
        private static Material EnsureEmissive(SetupUtils u, Material m)
        {
            if (m.IsKeywordEnabled("_EMISSION")) return m;
            m.EnableKeyword("_EMISSION");
            m.SetColor(MaterialUtility.EmissionColor, Color.white);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(m);
            u.Linked($"emission on {m.name}");
            return m;
        }

        /// <summary>Sets an object array field only if it is empty (never overwrites the PO's list).</summary>
        private static void LinkArrayIfEmpty(SetupUtils u, Object owner, string field, Object[] values)
        {
            var so = new SerializedObject(owner);
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null || prop.arraySize > 0) return;
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedProperties();
            u.Linked($"{owner.GetType().Name}.{field} ({values.Length})");
        }

        // ================================================================== Puzzles

        /// <summary>Shared materials and helpers for building the greybox puzzle pieces.</summary>
        private sealed class Parts
        {
            public readonly SetupUtils U;
            public readonly int Layer;
            public readonly TMP_FontAsset Font;
            public readonly Material Stone, Wood, Iron, Status, Pad, Icon;

            public Parts(SetupUtils u, int layer, TMP_FontAsset font, Material stone, Material wood, Material iron,
                Material status, Material pad, Material icon)
            {
                U = u; Layer = layer; Font = font; Stone = stone; Wood = wood; Iron = iron; Status = status; Pad = pad; Icon = icon;
            }

            /// <summary>A greybox primitive; solid pieces keep their collider.</summary>
            public static GameObject Piece(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale,
                Material material, bool solid)
            {
                GameObject go = GameObject.CreatePrimitive(type);
                go.name = name;
                if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(parent, false);
                go.transform.localPosition = position;
                go.transform.localScale = scale;
                go.GetComponent<Renderer>().sharedMaterial = material;
                return go;
            }

            public static Transform Anchor(Transform parent, string name, Vector3 position)
            {
                var t = new GameObject(name).transform;
                t.SetParent(parent, false);
                t.localPosition = position;
                return t;
            }

            public Renderer Orb(Transform model, float height) =>
                Piece(PrimitiveType.Sphere, model, "StatusOrb", new Vector3(0f, height, 0f), Vector3.one * 0.5f, Status, false).GetComponent<Renderer>();

            /// <summary>Floating "title / status" text above the site.</summary>
            public void Label(Transform site, Puzzle puzzle, float height)
            {
                if (site.Find("Label") != null) return;
                var go = new GameObject("Label");
                Undo.RegisterCreatedObjectUndo(go, "Setup");
                go.transform.SetParent(site, false);
                go.transform.localPosition = new Vector3(0f, height, 0f);
                var text = go.AddComponent<TextMeshPro>();
                text.font = Font;
                text.fontSize = 6f;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.sizeDelta = new Vector2(8f, 2f);
                text.text = puzzle.Title;
                var label = go.AddComponent<PuzzleLabel>();
                SetupUtils.SetValue(label, "puzzle", p => p.objectReferenceValue = puzzle);
                SetupUtils.SetValue(label, "text", p => p.objectReferenceValue = text);
                U.Created($"label above {site.name}");
            }

            /// <summary>The site's square on the minimap (tinted by the puzzle's status).</summary>
            public Renderer SiteIcon(Transform site)
            {
                GameObject icon = CreateIcon(site, Layer, PrimitiveType.Cube, null, Icon, new Vector3(4f, 0.02f, 4f), 0f,
                    PuzzleIconY, false, PuzzleColor, null);
                return icon.GetComponentInChildren<Renderer>();
            }
        }

        /// <summary>
        /// Shared start of every puzzle build: skip the site if it already has a puzzle, else add the
        /// puzzle component and a "Model" child for the greybox pieces.
        /// </summary>
        private static bool BeginPuzzle<T>(Parts parts, Transform site, out T puzzle, out Transform model) where T : Puzzle
        {
            puzzle = null;
            model = null;
            if (site == null) return false;
            if (site.TryGetComponent(out Puzzle existing))
            {
                parts.U.Skipped($"puzzle on {site.name}");
                puzzle = existing as T;
                return false;
            }
            puzzle = Undo.AddComponent<T>(site.gameObject);
            model = site.Find("Model");
            if (model == null) model = Parts.Anchor(site, "Model", Vector3.zero);
            return true;
        }

        /// <summary>Trigger zone matching the 6 m blue disc + kinematic Rigidbody (trigger events with the CharacterController).</summary>
        private static void AddZone(Transform site, float radius)
        {
            var zone = site.gameObject.AddComponent<SphereCollider>();
            zone.isTrigger = true;
            zone.radius = radius;
            zone.center = new Vector3(0f, 1f, 0f);
            var body = site.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private static void SetCommon(Puzzle puzzle, string title, params Renderer[] status)
        {
            var so = new SerializedObject(puzzle);
            so.FindProperty("title").stringValue = title;
            SerializedProperty list = so.FindProperty("statusRenderers");
            list.arraySize = status.Length;
            for (int i = 0; i < status.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = status[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSocket(ItemSocketPuzzle puzzle, ItemDefinition item, int required, bool openWhenSolved, params Transform[] slots)
        {
            var so = new SerializedObject(puzzle);
            so.FindProperty("item").objectReferenceValue = item;
            so.FindProperty("required").intValue = required;
            so.FindProperty("openWhenSolved").boolValue = openWhenSolved;
            SerializedProperty list = so.FindProperty("slots");
            list.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetOpening(Puzzle puzzle, Transform opening, Vector3 offset)
        {
            var so = new SerializedObject(puzzle);
            so.FindProperty("opening").objectReferenceValue = opening;
            so.FindProperty("openOffset").vector3Value = offset;
            so.FindProperty("openSeconds").floatValue = 1.2f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Puzzle 1 (PB-18): a door in a frame; the Key unlocks it and the door sinks into the ground.</summary>
        private static Puzzle BuildDoor(Parts p, Transform site, ItemDefinition key)
        {
            if (!BeginPuzzle(p, site, out ItemSocketPuzzle puzzle, out Transform model)) { if (puzzle != null) p.Label(site, puzzle, 4.6f); return puzzle; }
            Parts.Piece(PrimitiveType.Cube, model, "Post_L", new Vector3(-1.3f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.35f), p.Stone, true);
            Parts.Piece(PrimitiveType.Cube, model, "Post_R", new Vector3(1.3f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.35f), p.Stone, true);
            Parts.Piece(PrimitiveType.Cube, model, "Lintel", new Vector3(0f, 3.35f, 0f), new Vector3(2.95f, 0.35f, 0.35f), p.Stone, true);
            GameObject door = Parts.Piece(PrimitiveType.Cube, model, "Door", new Vector3(0f, 1.5f, 0f), new Vector3(2.25f, 3f, 0.18f), p.Wood, true);
            Transform slot = Parts.Anchor(model, "KeySlot", new Vector3(0f, 1.3f, -0.6f));
            Renderer orb = p.Orb(model, 3.9f);

            AddZone(site, 3f);
            SetCommon(puzzle, "Locked Door", orb, p.SiteIcon(site));
            SetSocket(puzzle, key, 1, true, slot);
            SetOpening(puzzle, door.transform, new Vector3(0f, -3.05f, 0f));
            p.Label(site, puzzle, 4.6f);
            p.U.Created($"puzzle 1 Locked Door (Key) on {site.name}");
            return puzzle;
        }

        /// <summary>Puzzle 2 (PB-19): an altar with three crystal slots.</summary>
        private static Puzzle BuildAltar(Parts p, Transform site, ItemDefinition crystal)
        {
            if (!BeginPuzzle(p, site, out ItemSocketPuzzle puzzle, out Transform model)) { if (puzzle != null) p.Label(site, puzzle, 3.6f); return puzzle; }
            Parts.Piece(PrimitiveType.Cube, model, "Altar", new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 1.2f), p.Stone, true);
            Transform a = Parts.Anchor(model, "CrystalSlot_1", new Vector3(-0.75f, 1.35f, 0f));
            Transform b = Parts.Anchor(model, "CrystalSlot_2", new Vector3(0f, 1.35f, 0f));
            Transform c = Parts.Anchor(model, "CrystalSlot_3", new Vector3(0.75f, 1.35f, 0f));
            Renderer orb = p.Orb(model, 2.6f);

            AddZone(site, 3f);
            SetCommon(puzzle, "Crystal Altar", orb, p.SiteIcon(site));
            SetSocket(puzzle, crystal, 3, false, a, b, c);
            p.Label(site, puzzle, 3.6f);
            p.U.Created($"puzzle 2 Crystal Altar (3 Crystals) on {site.name}");
            return puzzle;
        }

        /// <summary>Puzzle 3 (PB-20): a pillar with the status orb and two pressure switches either side.</summary>
        private static Puzzle BuildSwitches(Parts p, Transform site)
        {
            if (!BeginPuzzle(p, site, out SwitchPuzzle puzzle, out Transform model)) { if (puzzle != null) p.Label(site, puzzle, 3.6f); return puzzle; }
            Parts.Piece(PrimitiveType.Cube, model, "Pillar", new Vector3(0f, 1.2f, 0f), new Vector3(0.4f, 2.4f, 0.4f), p.Stone, true);
            Renderer orb = p.Orb(model, 2.7f);
            PressureSwitch left = BuildSwitch(p, site, "Switch_A", new Vector3(-2.2f, 0f, 0f));
            PressureSwitch right = BuildSwitch(p, site, "Switch_B", new Vector3(2.2f, 0f, 0f));

            SetCommon(puzzle, "Twin Switches", orb, p.SiteIcon(site));
            var so = new SerializedObject(puzzle);
            SerializedProperty list = so.FindProperty("switches");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = left;
            list.GetArrayElementAtIndex(1).objectReferenceValue = right;
            so.ApplyModifiedPropertiesWithoutUndo();
            p.Label(site, puzzle, 3.6f);
            p.U.Created($"puzzle 3 Twin Switches (2 pressure switches) on {site.name}");
            return puzzle;
        }

        /// <summary>A pressure switch: trigger + kinematic Rigidbody on the root, a stone base and a glowing pad.</summary>
        private static PressureSwitch BuildSwitch(Parts p, Transform site, string name, Vector3 position)
        {
            Transform root = Parts.Anchor(site, name, position);
            var zone = root.gameObject.AddComponent<BoxCollider>();
            zone.isTrigger = true;
            zone.size = new Vector3(1.4f, 1f, 1.4f);
            zone.center = new Vector3(0f, 0.5f, 0f);
            var body = root.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            // A Cylinder primitive is 2 m tall at scale 1, so scale y 0.05 = 10 cm.
            Parts.Piece(PrimitiveType.Cylinder, root, "Base", new Vector3(0f, 0.05f, 0f), new Vector3(1.5f, 0.05f, 1.5f), p.Stone, false);
            GameObject pad = Parts.Piece(PrimitiveType.Cylinder, root, "Pad", new Vector3(0f, 0.13f, 0f), new Vector3(1.1f, 0.04f, 1.1f), p.Pad, false);

            var sw = root.gameObject.AddComponent<PressureSwitch>();
            SetupUtils.SetValue(sw, "pad", prop => prop.objectReferenceValue = pad.transform);
            SetupUtils.SetValue(sw, "indicator", prop => prop.objectReferenceValue = pad.GetComponent<Renderer>());
            return sw;
        }

        /// <summary>Puzzle 4 (PB-21): a pedestal; the placed artifact appears on top (a copy of its pickup model).</summary>
        private static Puzzle BuildPedestal(Parts p, Transform site, ItemDefinition artifact)
        {
            if (!BeginPuzzle(p, site, out ItemSocketPuzzle puzzle, out Transform model)) { if (puzzle != null) p.Label(site, puzzle, 3.6f); return puzzle; }
            Parts.Piece(PrimitiveType.Cylinder, model, "Column", new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 0.55f, 0.7f), p.Stone, true);
            Parts.Piece(PrimitiveType.Cube, model, "Top", new Vector3(0f, 1.16f, 0f), new Vector3(1f, 0.12f, 1f), p.Stone, true);
            Transform slot = Parts.Anchor(model, "ArtifactSlot", new Vector3(0f, 1.4f, 0f));
            Renderer orb = p.Orb(model, 2.7f);

            AddZone(site, 3f);
            SetCommon(puzzle, "Artifact Pedestal", orb, p.SiteIcon(site));
            SetSocket(puzzle, artifact, 1, false, slot);
            p.Label(site, puzzle, 3.6f);
            p.U.Created($"puzzle 4 Artifact Pedestal (Artifact) on {site.name}");
            return puzzle;
        }

        /// <summary>Puzzle 5 (PB-22): a gate whose bars sink when puzzles 1–4 are solved; walking in solves it.</summary>
        private static Puzzle BuildGate(Parts p, Transform site, Puzzle[] prerequisites)
        {
            if (!BeginPuzzle(p, site, out GatePuzzle puzzle, out Transform model)) { if (puzzle != null) p.Label(site, puzzle, 5.8f); return puzzle; }
            Parts.Piece(PrimitiveType.Cube, model, "Pillar_L", new Vector3(-2.3f, 2f, 0f), new Vector3(0.7f, 4f, 0.7f), p.Stone, true);
            Parts.Piece(PrimitiveType.Cube, model, "Pillar_R", new Vector3(2.3f, 2f, 0f), new Vector3(0.7f, 4f, 0.7f), p.Stone, true);
            Parts.Piece(PrimitiveType.Cube, model, "Arch", new Vector3(0f, 4.3f, 0f), new Vector3(5.3f, 0.6f, 0.7f), p.Stone, true);
            GameObject bars = Parts.Piece(PrimitiveType.Cube, model, "Bars", new Vector3(0f, 1.95f, 0f), new Vector3(3.9f, 3.9f, 0.2f), p.Iron, true);
            Renderer orb = p.Orb(model, 5f);

            AddZone(site, 2.5f);
            SetCommon(puzzle, "Final Gate", orb, p.SiteIcon(site));
            SetOpening(puzzle, bars.transform, new Vector3(0f, -4f, 0f));
            var so = new SerializedObject(puzzle);
            SerializedProperty list = so.FindProperty("prerequisites");
            list.arraySize = prerequisites.Length;
            for (int i = 0; i < prerequisites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = prerequisites[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            p.Label(site, puzzle, 5.8f);
            p.U.Created($"puzzle 5 Final Gate (needs puzzles 1–4) on {site.name}");
            return puzzle;
        }

        // ================================================================== Minimap

        private static void SetupMinimap(SetupUtils u, Scene scene, int layer, Material icon, Transform hud, GameInput input,
            ItemDefinition key, ItemDefinition crystal, ItemDefinition artifact, TMP_FontAsset font)
        {
            Mesh arrow = GetOrCreateArrowMesh(u);
            RenderTexture texture = GetOrCreateRenderTexture(u);

            // Icons inside the prefabs, so every instance (and every dropped item) shows on the map.
            AddIconToPrefab(u, $"{Sprint2Setup.PrefabFolder}/Monster.prefab", layer, PrimitiveType.Cylinder, icon,
                new Vector3(2.4f, 0.01f, 2.4f), 0f, MonsterIconY, MonsterColor, linkHealth: true);
            foreach (ItemDefinition item in new[] { key, crystal, artifact })
            {
                if (item.PickupPrefab == null) { u.Warn($"{item.name} has no pickup prefab"); continue; }
                AddIconToPrefab(u, AssetDatabase.GetAssetPath(item.PickupPrefab), layer, PrimitiveType.Cube, icon,
                    new Vector3(1.6f, 0.02f, 1.6f), 45f, ItemIconY, item.Color, linkHealth: false);
            }

            Transform player = SetupUtils.FindRoot(scene, "Gameplay")?.Find("Player");
            if (player == null) u.Warn("Gameplay/Player not found: no player icon / minimap target");
            else if (player.GetComponentInChildren<MinimapIcon>(true) != null) u.Skipped("player minimap icon");
            else
            {
                GameObject arrowIcon = CreateIcon(player, layer, PrimitiveType.Quad, arrow, icon, new Vector3(3f, 1f, 3f), 0f,
                    PlayerIconY, true, PlayerColor, null);
                Undo.RegisterCreatedObjectUndo(arrowIcon, "Setup");
                u.Created("Player/MinimapIcon (arrow, turns with the player)");
            }

            AddStaticIcons(u, scene, layer,
                u.GetOrCreateUnlitMaterial("MI_Ground", new Color(0.13f, 0.12f, 0.15f)),
                u.GetOrCreateUnlitMaterial("MI_Wall", new Color(0.75f, 0.75f, 0.78f)));

            // Bit math on the culling mask: clear the Minimap bit so the main camera never draws the icons.
            Camera mainCamera = SetupUtils.FindRoot(scene, "Cameras")?.Find("Main Camera")?.GetComponent<Camera>();
            if (mainCamera == null) u.Warn("Cameras/Main Camera not found");
            else if ((mainCamera.cullingMask & (1 << layer)) != 0)
            {
                Undo.RecordObject(mainCamera, "Setup");
                mainCamera.cullingMask &= ~(1 << layer);
                u.Linked("Main Camera culling mask: Minimap layer excluded");
            }

            Camera minimapCamera = CreateMinimapCamera(u, scene, layer, texture, player);
            if (hud == null) return;
            GameObject panel = BuildMinimapPanel(u, hud, texture, font, key, crystal, artifact);

            var toggle = u.GetOrAdd<MinimapToggle>(hud.gameObject);
            u.LinkIfEmpty(toggle, "input", input);
            u.LinkIfEmpty(toggle, "panel", panel);
            u.LinkIfEmpty(toggle, "minimapCamera", minimapCamera);
        }

        /// <summary>Builds (once) the player's arrow mesh: a flat chevron in the XZ plane pointing +Z (PA1).</summary>
        private static Mesh GetOrCreateArrowMesh(SetupUtils u)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ArrowMeshPath);
            if (mesh != null)
            {
                u.Skipped(ArrowMeshPath);
                return mesh;
            }
            // Tip, right wing, inner notch, left wing; clockwise seen from above = front face up.
            mesh = new Mesh { name = "MinimapArrow" };
            mesh.vertices = new[] { new Vector3(0f, 0f, 1f), new Vector3(0.7f, 0f, -0.7f), new Vector3(0f, 0f, -0.35f), new Vector3(-0.7f, 0f, -0.7f) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, ArrowMeshPath);
            u.Created(ArrowMeshPath);
            return mesh;
        }

        private static RenderTexture GetOrCreateRenderTexture(SetupUtils u)
        {
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
            if (texture != null)
            {
                u.Skipped(RenderTexturePath);
                return texture;
            }
            // 512 × 512 colour target with a 16-bit depth buffer (higher icons hide lower ones).
            texture = new RenderTexture(512, 512, 16) { name = "RT_Minimap", antiAliasing = 2 };
            AssetDatabase.CreateAsset(texture, RenderTexturePath);
            u.Created(RenderTexturePath);
            return texture;
        }

        /// <summary>Adds a minimap icon inside a prefab asset, skipping prefabs that already have one.</summary>
        private static void AddIconToPrefab(SetupUtils u, string path, int layer, PrimitiveType shape, Material material,
            Vector3 scale, float yaw, float height, Color color, bool linkHealth)
        {
            // LoadPrefabContents opens the prefab in an isolated preview scene; finally always unloads it.
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponentInChildren<MinimapIcon>(true) != null)
                {
                    u.Skipped($"minimap icon in {path}");
                    return;
                }
                CreateIcon(root.transform, layer, shape, null, material, scale, yaw, height, false, color,
                    linkHealth ? root.GetComponent<Health>() : null);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                u.Created($"minimap icon in {path}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// Icon root (MinimapIcon: keeps level and height, colour) with a flat "Shape" child that keeps
        /// its own scale and yaw. Everything on the Minimap layer, no shadows.
        /// </summary>
        private static GameObject CreateIcon(Transform parent, int layer, PrimitiveType shape, Mesh customMesh, Material material,
            Vector3 scale, float yaw, float height, bool followYaw, Color color, Health health)
        {
            var icon = new GameObject("MinimapIcon");
            icon.transform.SetParent(parent, false);
            icon.transform.localPosition = Vector3.up * height;
            var component = icon.AddComponent<MinimapIcon>();
            SetupUtils.SetValue(component, "worldHeight", p => p.floatValue = height);
            SetupUtils.SetValue(component, "followYaw", p => p.boolValue = followYaw);
            SetupUtils.SetValue(component, "color", p => p.colorValue = color);
            if (health != null)
            {
                SetupUtils.SetValue(component, "health", p => p.objectReferenceValue = health);
                SetupUtils.SetValue(component, "depletedColor", p => p.colorValue = RecoveredColor);
            }

            GameObject shapeGo;
            if (customMesh != null)
            {
                shapeGo = new GameObject("Shape", typeof(MeshFilter), typeof(MeshRenderer));
                shapeGo.GetComponent<MeshFilter>().sharedMesh = customMesh;
                shapeGo.GetComponent<MeshRenderer>().sharedMaterial = material;
                shapeGo.transform.SetParent(icon.transform, false);
                shapeGo.transform.localScale = scale;
            }
            else shapeGo = SetupUtils.Visual(shape, icon.transform, "Shape", Vector3.zero, scale, material, new Vector3(0f, yaw, 0f));
            ConfigureIconRenderer(shapeGo.GetComponent<Renderer>());
            SetupUtils.SetLayerRecursively(icon, layer);
            return icon;
        }

        /// <summary>Icons are pure map markers: no shadows, no light or reflection probes.</summary>
        private static void ConfigureIconRenderer(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        /// <summary>
        /// Flat icons for the level: the ground (from its renderer) and the four walls (from their
        /// colliders: the wall renderers are hidden behind the fences since the Halloween theme).
        /// </summary>
        private static void AddStaticIcons(SetupUtils u, Scene scene, int layer, Material groundMaterial, Material wallMaterial)
        {
            Transform environment = SetupUtils.FindRoot(scene, "Environment");
            if (environment == null) { u.Warn("Environment not found: no static minimap icons"); return; }
            if (environment.Find("MinimapIcons") != null) { u.Skipped("Environment/MinimapIcons"); return; }

            Transform group = u.FindOrCreate(scene, "Environment/MinimapIcons").transform;
            Transform ground = environment.Find("Ground");
            if (ground != null && ground.TryGetComponent(out Renderer groundRenderer))
                StaticIcon(group, layer, groundMaterial, "Icon_Ground", groundRenderer.bounds, GroundIconY);
            Transform boundary = environment.Find("Boundary");
            if (boundary != null)
                foreach (Transform wall in boundary)
                    if (wall.name.StartsWith("Wall_") && wall.TryGetComponent(out Collider c))
                        StaticIcon(group, layer, wallMaterial, $"Icon_{wall.name}", c.bounds, WallIconY);
            u.Created($"static minimap icons ({group.childCount}: ground, walls)");
        }

        // Static icons have no MinimapIcon (they never move), so their colour comes from their own
        // material: a MaterialPropertyBlock set in the Editor is not saved with the scene.
        private static void StaticIcon(Transform parent, int layer, Material material, string name, Bounds b, float height)
        {
            GameObject go = SetupUtils.Visual(PrimitiveType.Cube, parent, name, Vector3.zero, new Vector3(b.size.x, 0.02f, b.size.z), material);
            go.transform.position = new Vector3(b.center.x, height, b.center.z);
            go.layer = layer;
            ConfigureIconRenderer(go.GetComponent<Renderer>());
        }

        /// <summary>
        /// Second camera for the minimap: orthographic, straight down, rendering only the Minimap layer
        /// into RT_Minimap; MinimapCamera follows the player at runtime.
        /// </summary>
        private static Camera CreateMinimapCamera(SetupUtils u, Scene scene, int layer, RenderTexture texture, Transform player)
        {
            Transform existing = SetupUtils.FindRoot(scene, "Cameras")?.Find("MinimapCamera");
            if (existing != null && existing.TryGetComponent(out Camera found))
            {
                u.Skipped("Cameras/MinimapCamera");
                return found;
            }

            GameObject go = u.FindOrCreate(scene, "Cameras/MinimapCamera");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 30f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.05f, 1f);
            cam.cullingMask = 1 << layer;
            cam.targetTexture = texture;
            cam.depth = -10f;   // renders before the main camera, so the texture is ready for the HUD
            cam.nearClipPlane = 1f;
            cam.farClipPlane = 60f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            go.transform.SetPositionAndRotation(new Vector3(0f, 40f, 0f), Quaternion.Euler(90f, 0f, 0f));

            // URP per-camera settings: flat icons need no shadows or post-processing.
            var urp = cam.GetUniversalAdditionalCameraData();
            urp.renderShadows = false;
            urp.renderPostProcessing = false;

            var follow = go.AddComponent<MinimapCamera>();
            SetupUtils.SetValue(follow, "target", p => p.objectReferenceValue = player);
            u.Created("Cameras/MinimapCamera (orthographic, Minimap layer only → RT_Minimap)");
            return cam;
        }

        /// <summary>HUD panel in the upper-right: the map (RawImage) and a colour legend.</summary>
        private static GameObject BuildMinimapPanel(SetupUtils u, Transform hud, RenderTexture texture, TMP_FontAsset font,
            ItemDefinition key, ItemDefinition crystal, ItemDefinition artifact)
        {
            Transform existing = hud.Find("MinimapPanel");
            if (existing != null)
            {
                u.Skipped("HUD/MinimapPanel");
                return existing.gameObject;
            }

            var panel = new GameObject("MinimapPanel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Setup");
            panel.SetParent(hud, false);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-30f, -30f);
            panel.sizeDelta = new Vector2(380f, 440f);
            var background = panel.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
            background.raycastTarget = false;

            var map = new GameObject("Map", typeof(RectTransform), typeof(RawImage)).GetComponent<RectTransform>();
            map.SetParent(panel, false);
            // Top edge of the panel, 360 × 360 (10 px border).
            map.anchorMin = map.anchorMax = map.pivot = new Vector2(0.5f, 1f);
            map.anchoredPosition = new Vector2(0f, -10f);
            map.sizeDelta = new Vector2(360f, 360f);
            var raw = map.GetComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;

            string Swatch(Color c, string label) => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{label}</color>";
            TMP_Text legend = Sprint1Setup.Text(u, panel, "Legend",
                $"{Swatch(PlayerColor, "You")}  {Swatch(PuzzleColor, "Puzzle")}  {Swatch(SolvedColor, "Solved")}  {Swatch(MonsterColor, "Monster")}\n" +
                $"{Swatch(key.Color, key.DisplayName)}  {Swatch(crystal.Color, crystal.DisplayName)}  {Swatch(artifact.Color, artifact.DisplayName)}   <color=#BBBBBB>M: hide map</color>",
                20, new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(370f, 56f), TextAlignmentOptions.Center, font);
            legend.raycastTarget = false;
            u.Created("HUD/MinimapPanel (upper-right, 360 px map + legend)");
            return panel.gameObject;
        }
    }
}
