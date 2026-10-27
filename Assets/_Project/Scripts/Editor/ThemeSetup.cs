using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PuzzleMaster.Core;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Halloween theme setup (PB-26, ADR-0011 / ADR-0013), using PA1's assets. Idempotent: each part
    /// is skipped once done, so rerunning never stacks models or overwrites tuned lighting.
    /// - Monster prefab: the greybox body / hat / eyes in `Model` are replaced by the PA1 ghost
    ///   (only `Model`'s children change, so Monster.model stays linked); collider and life bar fitted.
    /// - Props (Environment/Props/Prop_nn): greybox cubes become tombstones (collider fitted).
    /// - Puzzle sites: two jack-o'-lantern pumpkins + a warm point light each, as night beacons.
    /// - Night: Midnight skybox + PA1's exact lighting (moonlight, flat ambient, purple fog, a faint
    ///   player fill light) and PA1's post-processing profile (DoF, bloom, colour grading, vignette).
    /// - Recovered ghosts float up and fade: M_GhostFade template linked into Monster.prefab (ADR-0014).
    /// - Trees: a ring outside the fence (on extended dirt ground) and two in empty corners inside.
    /// - Spells glow: an HDR unlit purple core (bloom), a small purple point light, a fading additive trail.
    /// - The red monster-spot discs are hidden (the ghosts stand there now; Sprint 2 retro).
    /// - Ground: the graveyard's tileable dirt texture (+ normal map) on the flat ground plane.
    /// - Walls: PA1 fence models lined up along the four boundary walls; the wall cubes stay as
    ///   invisible colliders, so movement and the camera behave exactly as before.
    /// Menu: Puzzle Master → Setup → Halloween Theme
    /// Batch: -executeMethod PuzzleMaster.EditorTools.ThemeSetup.RunBatch
    /// </summary>
    public static class ThemeSetup
    {
        private const string Halloween = SetupUtils.ProjectRoot + "/Models/Halloween";
        private const string GhostPath = Halloween + "/ghost.fbx";
        private const string TombstonePath = Halloween + "/tombstone.fbx";
        private const string PumpkinPath = Halloween + "/pumpkin.fbx";
        private const string FencePath = Halloween + "/fence.fbx";
        private const string DirtBasePath = Halloween + "/textures/tile_dirt_BaseColor.png";
        private const string DirtNormalPath = Halloween + "/textures/tile_dirt_Normal.png";
        private const string GroundMaterialPath = SetupUtils.ProjectRoot + "/Materials/M_GroundDirt.mat";
        private const string TreePath = Halloween + "/tree.fbx";
        private const string FadeMaterialPath = SetupUtils.ProjectRoot + "/Materials/M_GhostFade.mat";
        private const string PostProfilePath = SetupUtils.ProjectRoot + "/Settings/PostProcess Profile.asset";
        private const string MonsterPrefabPath = Sprint2Setup.PrefabFolder + "/Monster.prefab";
        private const string SpellPrefabPath = Sprint2Setup.PrefabFolder + "/SpellProjectile.prefab";
        private const string SpellCoreMaterialPath = SetupUtils.ProjectRoot + "/Materials/M_SpellCore.mat";
        private const string SpellTrailMaterialPath = SetupUtils.ProjectRoot + "/Materials/M_SpellTrail.mat";
        private static readonly Color SpellColor = new Color(0.8f, 0.4f, 1f);
        private const string SkyboxPath = "Assets/Free Stylized Hand-Painted Skybox/Materials/Midnight.mat";

        // Target sizes in metres (the FBXs are scaled to these, measured from their renderer bounds).
        private const float GhostHeight = 1.9f, GhostHover = 0.2f, TombstoneHeight = 1.5f, PumpkinHeight = 0.6f;
        // Fences match the 3 m wall cubes; one dirt tile covers DirtTileSize metres of ground.
        private const float FenceHeight = 3f, DirtTileSize = 4f;
        private static readonly string[] WallNames = { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };

        private static readonly string[] GreyboxMonsterParts = { "Body", "HatBrim", "HatTop", "Eye_L", "Eye_R" };
        // Pumpkin beacons at each puzzle site (local offsets from the site centre, just outside the 3 m disc).
        private static readonly Vector3[] PumpkinOffsets = { new Vector3(-3.6f, 0f, 3.2f), new Vector3(3.6f, 0f, 3.2f) };

        [MenuItem("Puzzle Master/Setup/Halloween Theme")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Halloween theme", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string log = Run();
            EditorUtility.DisplayDialog("Halloween theme finished",
                "Done. See the Console for the report.\n\nNext: press Play and follow docs/guides/halloween-theme.md.", "OK");
            Debug.Log(log);
        }

        public static void RunBatch() => Debug.Log(Run());

        private static string Run()
        {
            var u = new SetupUtils();
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != Sprint0Setup.MainScenePath)
                scene = EditorSceneManager.OpenScene(Sprint0Setup.MainScenePath, OpenSceneMode.Single);

            var ghost = AssetDatabase.LoadAssetAtPath<GameObject>(GhostPath);
            var tombstone = AssetDatabase.LoadAssetAtPath<GameObject>(TombstonePath);
            var pumpkin = AssetDatabase.LoadAssetAtPath<GameObject>(PumpkinPath);
            var fence = AssetDatabase.LoadAssetAtPath<GameObject>(FencePath);
            var tree = AssetDatabase.LoadAssetAtPath<GameObject>(TreePath);
            if (ghost == null || tombstone == null || pumpkin == null || fence == null || tree == null)
            {
                u.Warn($"PA1 Halloween models missing under {Halloween}");
                return Report(u, scene);
            }

            SwapMonsterToGhost(u, ghost);
            SwapPropsToTombstones(u, scene, tombstone);
            AddPumpkinBeacons(u, scene, pumpkin);
            ApplyNight(u, scene);
            HideMonsterSpotMarkers(u, scene);
            ApplyDirtGround(u, scene);
            LineWallsWithFences(u, scene, fence);
            LinkGhostFade(u);
            AddTrees(u, scene, tree);
            GlowSpells(u);
            return Report(u, scene);
        }

        // ================================================================== Models

        /// <summary>
        /// Instantiates an FBX under <paramref name="parent"/>, keeps the asset's own import rotation
        /// (these FBXs need it to stand upright) plus an extra yaw, scales it to <paramref name="height"/>
        /// and puts its lowest point at <paramref name="lift"/> above the parent's origin.
        /// </summary>
        private static GameObject PlaceModel(GameObject asset, Transform parent, string name, float height, float yaw, float lift = 0f)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            go.name = name;
            Transform t = go.transform;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f) * asset.transform.localRotation;
            t.localPosition = Vector3.zero;
            // Measure at the asset's own import scale (these FBXs carry a root scale of ~100), then
            // multiply by the factor that gives the target height. Resetting to 1 first would apply it twice.
            t.localScale = asset.transform.localScale;
            Bounds b = WorldBounds(go);
            t.localScale = asset.transform.localScale * (height / Mathf.Max(0.0001f, b.size.y));
            b = WorldBounds(go);
            // Move so the bottom sits at parent + lift (and the model is centred over the pivot).
            Vector3 pivot = parent.position + parent.up * lift;
            t.position += new Vector3(pivot.x - b.center.x, pivot.y - b.min.y, pivot.z - b.center.z);
            return go;
        }

        private static Bounds WorldBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        /// <summary>Replaces the greybox parts in Monster.prefab's Model with the ghost; fits collider, life bar, cast point.</summary>
        private static void SwapMonsterToGhost(SetupUtils u, GameObject ghost)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(MonsterPrefabPath);
            try
            {
                Transform model = root.transform.Find("Model");
                if (model == null || model.Find("Ghost") != null)
                {
                    u.Skipped(model == null ? "Monster.prefab has no Model (skipped)" : "Monster.prefab ghost");
                    return;
                }
                foreach (string part in GreyboxMonsterParts)
                {
                    Transform t = model.Find(part);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
                // The ghost model faces +Z like the monster (checked by a render), hovering slightly.
                PlaceModel(ghost, model, "Ghost", GhostHeight, 0f, GhostHover);

                if (root.TryGetComponent(out CapsuleCollider capsule))
                {
                    capsule.height = GhostHeight + GhostHover;
                    capsule.center = new Vector3(0f, capsule.height / 2f, 0f);
                    capsule.radius = 0.6f;
                }
                Transform lifeBar = root.transform.Find("LifeBar");
                if (lifeBar != null) lifeBar.localPosition = new Vector3(0f, GhostHeight + GhostHover + 0.35f, 0f);
                Transform castPoint = root.transform.Find("CastPoint");
                if (castPoint != null) castPoint.localPosition = new Vector3(0f, 1.3f, 0.8f);

                PrefabUtility.SaveAsPrefabAsset(root, MonsterPrefabPath);
                u.Linked("Monster.prefab: Model → PA1 ghost (greybox parts removed; collider, life bar, cast point fitted)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Greybox prop cubes → tombstones: the root keeps its position and BoxCollider (fitted to the stone).</summary>
        private static void SwapPropsToTombstones(SetupUtils u, Scene scene, GameObject tombstone)
        {
            Transform props = SetupUtils.FindRoot(scene, "Environment")?.Find("Props");
            if (props == null)
            {
                u.Warn("Environment/Props not found");
                return;
            }
            int swapped = 0, index = 0;
            foreach (Transform prop in props.Cast<Transform>().ToList())
            {
                index++;
                if (prop.Find("Tombstone") != null) continue;
                // Greybox cube: drop its mesh, reset its (non-uniform) scale so the model isn't stretched.
                Undo.RecordObject(prop, "Setup");
                prop.localScale = Vector3.one;
                prop.position = new Vector3(prop.position.x, 0f, prop.position.z);   // cubes were centred at half height
                if (prop.TryGetComponent(out MeshRenderer meshRenderer)) Undo.DestroyObjectImmediate(meshRenderer);
                if (prop.TryGetComponent(out MeshFilter meshFilter)) Undo.DestroyObjectImmediate(meshFilter);

                // Varied yaw per stone (deterministic from the index, so reruns match).
                GameObject stone = PlaceModel(tombstone, prop, "Tombstone", TombstoneHeight, index * 47f % 360f - 180f);
                Undo.RegisterCreatedObjectUndo(stone, "Setup");

                // TryGetComponent, not "GetComponent() ?? …": in the Editor a missing component is a fake
                // null object that "??" does not treat as null.
                if (!prop.TryGetComponent(out BoxCollider box)) box = Undo.AddComponent<BoxCollider>(prop.gameObject);
                Undo.RecordObject(box, "Setup");
                Bounds b = WorldBounds(stone);
                box.center = prop.InverseTransformPoint(b.center);
                box.size = b.size;
                swapped++;
            }
            if (swapped > 0) u.Linked($"{swapped} props → tombstones (colliders fitted)");
            else u.Skipped("props already tombstones");
        }

        /// <summary>Two pumpkins and a warm light per puzzle site: night-time beacons that mark the puzzles.</summary>
        private static void AddPumpkinBeacons(SetupUtils u, Scene scene, GameObject pumpkin)
        {
            Transform sites = SetupUtils.FindRoot(scene, "Gameplay")?.Find("PuzzleSites");
            if (sites == null)
            {
                u.Warn("Gameplay/PuzzleSites not found");
                return;
            }
            Transform decor = u.FindOrCreate(scene, "Environment/Decor/PumpkinBeacons").transform;
            foreach (Transform site in sites)
            {
                string groupName = $"Beacon_{site.name.Replace("PuzzleSite_", "")}";
                if (decor.Find(groupName) != null) continue;

                var group = new GameObject(groupName).transform;
                Undo.RegisterCreatedObjectUndo(group.gameObject, "Setup");
                group.SetParent(decor, false);
                group.SetPositionAndRotation(site.position, site.rotation);
                for (int i = 0; i < PumpkinOffsets.Length; i++)
                {
                    var spot = new GameObject($"Pumpkin_{i + 1}").transform;
                    spot.SetParent(group, false);
                    spot.localPosition = PumpkinOffsets[i];
                    // Face the site centre (the carved face points +Z on this model).
                    spot.localRotation = Quaternion.LookRotation(-PumpkinOffsets[i].normalized);
                    GameObject model = PlaceModel(pumpkin, spot, "Model", PumpkinHeight, 0f);
                    Bounds b = WorldBounds(model);
                    var box = spot.gameObject.AddComponent<BoxCollider>();
                    box.center = spot.InverseTransformPoint(b.center);
                    box.size = new Vector3(b.size.x, b.size.y, b.size.z);
                }

                var lamp = new GameObject("Glow").AddComponent<Light>();
                lamp.transform.SetParent(group, false);
                lamp.transform.localPosition = new Vector3(0f, 1.2f, 3.2f);
                lamp.type = LightType.Point;
                lamp.color = new Color(1f, 0.55f, 0.15f);
                lamp.range = 7f;
                lamp.intensity = 2f;
                lamp.shadows = LightShadows.None;   // cheap; five of these are fine in URP Forward+
                u.Created($"Environment/Decor/PumpkinBeacons/{groupName} (2 pumpkins + warm light)");
            }
        }

        // ================================================================== Night

        /// <summary>
        /// PA1's exact night look (read from PA1's Main.unity): Midnight skybox, flat dark ambient,
        /// purple exponential-squared fog, moonlight, a faint fill light on the player, and PA1's
        /// post-processing profile on a global Volume (camera post-processing on). Applied once:
        /// skipped when Environment/PostProcess exists, so your later tuning is kept.
        /// </summary>
        private static void ApplyNight(SetupUtils u, Scene scene)
        {
            Transform environment = SetupUtils.FindRoot(scene, "Environment");
            if (environment == null) return;
            // Look for any global Volume, not a fixed path: the PO may reorganise the hierarchy
            // (e.g. Environment/Lighting/PostProcess); a path check would re-apply and duplicate it.
            Volume existing = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(v => v.isGlobal && v.gameObject.scene == scene);
            if (existing != null)
            {
                u.Skipped($"PA1 night look + post-processing ({existing.name} exists; lighting left as tuned)");
                return;
            }

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
            if (skybox != null) RenderSettings.skybox = skybox;
            else u.Warn($"Skybox not found ({SkyboxPath}): an Asset Store pack, git-ignored; import it via Package Manager → My Assets");

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.0118f, 0.0161f, 0.0392f);
            RenderSettings.reflectionIntensity = 0.3f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.1833f, 0.1098f, 0.2f);
            RenderSettings.fogDensity = 0.02f;

            Light moon = environment.GetComponentsInChildren<Light>(true).FirstOrDefault(l => l.type == LightType.Directional);
            if (moon != null)
            {
                Undo.RecordObject(moon, "Setup");
                Undo.RecordObject(moon.transform, "Setup");
                moon.color = new Color(0.5608f, 0.6588f, 1f);
                moon.intensity = 0.5f;
                moon.shadows = LightShadows.Soft;
                moon.shadowStrength = 0.6f;
                moon.transform.rotation = Quaternion.Euler(25f, -40f, 0f);
            }

            // PA1's faint white fill light that travels with the player (keeps the witch readable at night).
            Transform player = SetupUtils.FindRoot(scene, "Gameplay")?.Find("Player");
            if (player != null && player.Find("FillLight") == null)
            {
                var fill = new GameObject("FillLight").AddComponent<Light>();
                Undo.RegisterCreatedObjectUndo(fill.gameObject, "Setup");
                fill.transform.SetParent(player, false);
                fill.transform.localPosition = new Vector3(0f, 1f, 0.57f);
                fill.type = LightType.Point;
                fill.color = Color.white;
                fill.intensity = 0.05f;
                fill.range = 10f;
                fill.shadows = LightShadows.None;
                fill.gameObject.layer = player.gameObject.layer;
            }

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostProfilePath);
            var volumeGo = new GameObject("PostProcess");
            Undo.RegisterCreatedObjectUndo(volumeGo, "Setup");
            volumeGo.transform.SetParent(environment, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            if (profile == null) u.Warn($"{PostProfilePath} missing: Volume created without a profile");

            Camera main = SetupUtils.FindRoot(scene, "Cameras")?.GetComponentsInChildren<Camera>(true).FirstOrDefault(c => c.CompareTag("MainCamera"));
            if (main != null)
            {
                UniversalAdditionalCameraData data = main.GetUniversalAdditionalCameraData();
                Undo.RecordObject(data, "Setup");
                data.renderPostProcessing = true;
            }
            u.Linked("PA1 night look: Midnight skybox, flat ambient, purple fog 0.02, moonlight 0.5, player fill light, " +
                     "post-processing Volume (DoF 15–45 m, bloom, colour grading, white balance, vignette), camera post on");
        }

        private static void HideMonsterSpotMarkers(SetupUtils u, Scene scene)
        {
            Transform spots = SetupUtils.FindRoot(scene, "Gameplay")?.Find("MonsterSpots");
            if (spots == null) return;
            int hidden = 0;
            foreach (Transform spot in spots)
            {
                Transform marker = spot.Find("Marker");
                if (marker == null || !marker.gameObject.activeSelf) continue;
                Undo.RecordObject(marker.gameObject, "Setup");
                marker.gameObject.SetActive(false);
                hidden++;
            }
            if (hidden > 0) u.Linked($"{hidden} red monster-spot markers hidden");
        }

        // ================================================================== Ground + walls

        /// <summary>Dirt material (base + normal map, tiled every DirtTileSize m) on Environment/Ground.</summary>
        private static void ApplyDirtGround(SetupUtils u, Scene scene)
        {
            Transform ground = SetupUtils.FindRoot(scene, "Environment")?.Find("Ground");
            if (ground == null || !ground.TryGetComponent(out MeshRenderer renderer))
            {
                u.Warn("Environment/Ground (MeshRenderer) not found");
                return;
            }

            // The normal map was imported as a colour texture; as a bump map it must be a Normal map.
            var normalImporter = (TextureImporter)AssetImporter.GetAtPath(DirtNormalPath);
            if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
                u.Linked("tile_dirt_Normal imported as a Normal map");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(GroundMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_GroundDirt" };
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DirtBasePath));
                material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DirtNormalPath));
                material.EnableKeyword("_NORMALMAP");
                material.SetFloat("_Smoothness", 0.1f);
                // Tiling from the ground's world size (a Plane is 10 m × scale).
                Vector3 size = ground.lossyScale * 10f;
                material.SetTextureScale("_BaseMap", new Vector2(size.x / DirtTileSize, size.z / DirtTileSize));
                AssetDatabase.CreateAsset(material, GroundMaterialPath);
                u.Created($"{GroundMaterialPath} (dirt, tiled {size.x / DirtTileSize:0} × {size.z / DirtTileSize:0})");
            }
            if (renderer.sharedMaterial == material)
            {
                u.Skipped("ground dirt material");
                return;
            }
            Undo.RecordObject(renderer, "Setup");
            renderer.sharedMaterial = material;
            u.Linked("Environment/Ground → M_GroundDirt");
        }

        /// <summary>
        /// Fence segments along each boundary wall (in a sibling group, not as children: the wall cubes are
        /// non-uniformly scaled). The wall's renderer is turned off; its BoxCollider still blocks the player.
        /// </summary>
        private static void LineWallsWithFences(SetupUtils u, Scene scene, GameObject fence)
        {
            Transform boundary = SetupUtils.FindRoot(scene, "Environment")?.Find("Boundary");
            if (boundary == null)
            {
                u.Warn("Environment/Boundary not found");
                return;
            }

            // Measure one fence at the target height to know the segment length (its long side).
            GameObject probe = PlaceModel(fence, boundary, "Probe", FenceHeight, 0f);
            Bounds pb = WorldBounds(probe);
            float segment = Mathf.Max(pb.size.x, pb.size.z);
            Object.DestroyImmediate(probe);

            foreach (string wallName in WallNames)
            {
                Transform wall = boundary.Find(wallName);
                string groupName = wallName.Replace("Wall_", "Fence_");
                if (wall == null || boundary.Find(groupName) != null) continue;

                // The wall's long axis (x for north/south, z for east/west) and its length.
                bool alongX = wall.lossyScale.x > wall.lossyScale.z;
                float length = alongX ? wall.lossyScale.x : wall.lossyScale.z;
                int count = Mathf.CeilToInt(length / segment);
                float step = length / count;
                // The fence model's long side is its local z (measured above), so walls along x need a 90° turn.
                float yaw = (alongX ? 90f : 0f) + (pb.size.x > pb.size.z ? 90f : 0f);

                var group = new GameObject(groupName).transform;
                Undo.RegisterCreatedObjectUndo(group.gameObject, "Setup");
                group.SetParent(boundary, false);
                Vector3 axis = alongX ? Vector3.right : Vector3.forward;
                Vector3 start = new Vector3(wall.position.x, 0f, wall.position.z) - axis * (length / 2f - step / 2f);
                for (int i = 0; i < count; i++)
                {
                    var slot = new GameObject($"Fence_{i + 1:00}").transform;
                    slot.SetParent(group, false);
                    slot.position = start + axis * (step * i);
                    PlaceModel(fence, slot, "Model", FenceHeight, yaw);
                }

                if (wall.TryGetComponent(out MeshRenderer wallRenderer) && wallRenderer.enabled)
                {
                    Undo.RecordObject(wallRenderer, "Setup");
                    wallRenderer.enabled = false;   // invisible wall: the collider stays
                }
                u.Created($"Environment/Boundary/{groupName}: {count} fence segments ({step:0.0} m); {wallName} renderer off, collider kept");
            }
        }

        // ================================================================== Spells

        /// <summary>
        /// Makes monster spells glow: an HDR unlit core (above the bloom threshold, so the post profile's
        /// bloom gives it a halo), a purple point light that lights the ground as it flies,
        /// and a short additive trail. Each part is skipped once present.
        /// </summary>
        private static void GlowSpells(SetupUtils u)
        {
            Material core = GetOrCreateSpellCoreMaterial(u);
            Material trailMaterial = GetOrCreateTrailMaterial(u);
            GameObject root = PrefabUtility.LoadPrefabContents(SpellPrefabPath);
            try
            {
                bool changed = false;
                Renderer model = root.transform.Find("Model")?.GetComponent<Renderer>();
                if (model == null) u.Warn("SpellProjectile.prefab has no Model renderer");
                else if (model.sharedMaterial != core && model.sharedMaterial != null && model.sharedMaterial.name == "M_Spell")
                {
                    model.sharedMaterial = core;
                    changed = true;
                    u.Linked("SpellProjectile.prefab/Model → M_SpellCore (glowing core)");
                }
                if (root.transform.Find("Glow") == null)
                {
                    var glow = new GameObject("Glow").AddComponent<Light>();
                    glow.transform.SetParent(root.transform, false);
                    glow.type = LightType.Point;
                    glow.color = SpellColor;
                    glow.range = 4f;
                    glow.intensity = 3f;
                    glow.shadows = LightShadows.None;   // a few short-lived lights are cheap in URP Forward+
                    changed = true;
                    u.Created("SpellProjectile.prefab/Glow (purple point light)");
                }
                if (!root.TryGetComponent(out TrailRenderer trail))
                {
                    trail = root.AddComponent<TrailRenderer>();
                    trail.sharedMaterial = trailMaterial;
                    trail.time = 0.3f;
                    trail.minVertexDistance = 0.1f;
                    trail.widthCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 0f);   // thick at the spell, thin at the tail
                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new[] { new GradientColorKey(SpellColor, 0f), new GradientColorKey(SpellColor, 1f) },
                        new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
                    trail.colorGradient = gradient;
                    trail.shadowCastingMode = ShadowCastingMode.Off;
                    trail.receiveShadows = false;
                    changed = true;
                    u.Created("SpellProjectile.prefab trail (0.3 s, fading)");
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, SpellPrefabPath);
                else u.Skipped("spell glow light + trail");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// URP Unlit with an HDR colour (purple × 1.3): unlit output goes straight to the HDR buffer,
        /// above the 0.9 bloom threshold, so bloom draws the halo. Only slightly over 1: the post profile
        /// has no tonemapping, so stronger values clip every channel to white. No shader keyword needed (an emission
        /// keyword set from a batch run was not always saved; the unlit colour is just a property).
        /// </summary>
        private static Material GetOrCreateSpellCoreMaterial(SetupUtils u)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SpellCoreMaterialPath);
            if (material != null)
            {
                u.Skipped(SpellCoreMaterialPath);
                return material;
            }
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "M_SpellCore" };
            material.SetColor(MaterialUtility.BaseColor, new Color(SpellColor.r * 1.3f, SpellColor.g * 1.3f, SpellColor.b * 1.3f, 1f));
            AssetDatabase.CreateAsset(material, SpellCoreMaterialPath);
            u.Created($"{SpellCoreMaterialPath} (URP Unlit, HDR purple × 1.3)");
            return material;
        }

        /// <summary>URP Particles/Unlit, transparent additive: the trail brightens what is behind it and never darkens.</summary>
        private static Material GetOrCreateTrailMaterial(SetupUtils u)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SpellTrailMaterialPath);
            if (material != null)
            {
                u.Skipped(SpellTrailMaterialPath);
                return material;
            }
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "M_SpellTrail" };
            // Like MaterialUtility.MakeTransparent, but additive (dst × One): set what the Inspector would set.
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);   // 2 = Additive in URP's particle shaders
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetColor(MaterialUtility.BaseColor, Color.white);   // the trail's vertex colours carry the purple
            AssetDatabase.CreateAsset(material, SpellTrailMaterialPath);
            u.Created($"{SpellTrailMaterialPath} (URP Particles/Unlit, additive)");
            return material;
        }

        // ================================================================== Fade + trees

        /// <summary>Creates M_GhostFade (transparent URP Lit, normal map + emission) and links it into Monster.prefab.</summary>
        private static void LinkGhostFade(SetupUtils u)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(FadeMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_GhostFade" };
                MaterialUtility.MakeTransparent(material);
                // Enabling the keywords on the asset keeps those transparent variants in builds (PA1 ADR-0010).
                material.EnableKeyword("_NORMALMAP");
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                AssetDatabase.CreateAsset(material, FadeMaterialPath);
                u.Created($"{FadeMaterialPath} (URP Lit, Transparent, normal map + emission)");
            }
            GameObject root = PrefabUtility.LoadPrefabContents(MonsterPrefabPath);
            try
            {
                var monster = root.GetComponent<Monsters.Monster>();
                var so = new SerializedObject(monster);
                SerializedProperty template = so.FindProperty("fadeMaterialTemplate");
                if (template.objectReferenceValue != null)
                {
                    u.Skipped("Monster.prefab fade template");
                    return;
                }
                template.objectReferenceValue = material;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, MonsterPrefabPath);
                u.Linked("Monster.prefab: Fade Material Template → M_GhostFade (recovered ghosts float up and fade)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// A ring of trees outside the fence, on a larger dirt plane (so the area beyond the bars isn't empty),
        /// plus two trees with trunk colliders in empty corners inside. Deterministic layout (fixed seed).
        /// </summary>
        private static void AddTrees(SetupUtils u, Scene scene, GameObject tree)
        {
            Transform environment = SetupUtils.FindRoot(scene, "Environment");
            if (environment == null || environment.Find("Trees") != null)
            {
                if (environment != null) u.Skipped("Environment/Trees");
                return;
            }

            // Outer ground: same dirt, slightly below the play ground, no collider (unreachable).
            var dirt = AssetDatabase.LoadAssetAtPath<Material>(GroundMaterialPath);
            GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(outer, "Setup");
            outer.name = "OuterGround";
            outer.transform.SetParent(environment, false);
            outer.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            outer.transform.localScale = new Vector3(16f, 1f, 16f);   // 160 × 160 m
            Object.DestroyImmediate(outer.GetComponent<Collider>());
            if (dirt != null)
            {
                var outerDirt = new Material(dirt) { name = "M_GroundDirtOuter" };
                outerDirt.SetTextureScale("_BaseMap", new Vector2(160f / DirtTileSize, 160f / DirtTileSize));
                string outerPath = SetupUtils.ProjectRoot + "/Materials/M_GroundDirtOuter.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(outerPath) == null) AssetDatabase.CreateAsset(outerDirt, outerPath);
                outer.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(outerPath);
            }

            Transform trees = new GameObject("Trees").transform;
            Undo.RegisterCreatedObjectUndo(trees.gameObject, "Setup");
            trees.SetParent(environment, false);
            var random = new System.Random(42);
            float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);

            // Outside ring: along each side of a 100 m square, every ~10 m, jittered.
            int outside = 0;
            for (int side = 0; side < 4; side++)
            {
                for (float along = -50f; along < 50f; along += 10f)
                {
                    float a = along + Range(-3f, 3f), d = Range(46f, 54f);
                    Vector3 p = side switch
                    {
                        0 => new Vector3(a, 0f, d),
                        1 => new Vector3(a, 0f, -d),
                        2 => new Vector3(d, 0f, a),
                        _ => new Vector3(-d, 0f, a),
                    };
                    var slot = new GameObject($"Tree_Out_{++outside:00}").transform;
                    slot.SetParent(trees, false);
                    slot.position = p;
                    PlaceModel(tree, slot, "Model", Range(6.5f, 9.5f), Range(0f, 360f));
                }
            }

            // Inside: two corners that are free of puzzle sites, monsters, props and the player start.
            Vector3[] inside = { new Vector3(-34f, 0f, 34f), new Vector3(34f, 0f, -34f) };
            for (int i = 0; i < inside.Length; i++)
            {
                var slot = new GameObject($"Tree_In_{i + 1}").transform;
                slot.SetParent(trees, false);
                slot.position = inside[i];
                PlaceModel(tree, slot, "Model", 8f, Range(0f, 360f));
                var trunk = slot.gameObject.AddComponent<CapsuleCollider>();
                trunk.center = new Vector3(0f, 2f, 0f);
                trunk.height = 4f;
                trunk.radius = 0.5f;
            }
            u.Created($"Environment/Trees: {outside} outside the fence + {inside.Length} inside (trunk colliders); OuterGround 160 × 160 m dirt");
        }

        private static string Report(SetupUtils u, Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Halloween theme setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }
    }
}
