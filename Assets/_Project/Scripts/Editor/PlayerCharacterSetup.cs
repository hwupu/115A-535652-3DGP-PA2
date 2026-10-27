using System.Linq;
using PuzzleMaster.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleMaster.EditorTools
{
    /// <summary>
    /// Player character setup (PB-26, ADR-0012): the Little Witch from PA1 with Idle / Walk / Run
    /// and the new MeleeSwing clip. Simple by design (PA1 ADR-0011 style). Idempotent.
    /// Steps: import the swing clip FBX (Generic, avatar copied from the witch), add the Attack
    /// trigger + MeleeSwing state to AC_Player, create M_Witch, put the witch under Player/Model
    /// (greybox body and nose hidden), move the sword into the witch's right hand, add PlayerAnimator.
    /// Menu: Puzzle Master → Setup → Player Character (Witch)
    /// Batch: -executeMethod PuzzleMaster.EditorTools.PlayerCharacterSetup.RunBatch
    /// </summary>
    public static class PlayerCharacterSetup
    {
        private const string ModelFolder = SetupUtils.ProjectRoot + "/Models/Cute Witch";
        private const string ModelPath = ModelFolder + "/The little Witch.fbx";
        private const string SwingClipPath = ModelFolder + "/The little Witch@MeleeSwing.fbx";
        private const string TexturePath = ModelFolder + "/textures/Material_Base_Color.png";
        private const string MaterialPath = SetupUtils.ProjectRoot + "/Materials/M_Witch.mat";
        private const string ControllerPath = SetupUtils.ProjectRoot + "/Animation/AC_Player.controller";
        private const string SwingClip = "MeleeSwing";
        private const string AttackTrigger = "Attack";

        // The witch is ~1.33 m tall (incl. hat); 1.35 makes her ~1.8 m, the CharacterController height.
        private const float ModelScale = 1.35f;
        // Sword held in the right fist: blade forward, grip at the hand (set once in the bind pose).
        private const float SwordGripOffset = 0.4f;

        [MenuItem("Puzzle Master/Setup/Player Character (Witch)")]
        public static void RunFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Player character setup", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string log = Run();
            EditorUtility.DisplayDialog("Player character setup finished",
                "Done. See the Console for the report.\n\nNext: press Play and follow docs/guides/witch-melee-swing.md.", "OK");
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

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) == null || AssetDatabase.LoadAssetAtPath<GameObject>(SwingClipPath) == null)
            {
                u.Warn($"Missing {ModelPath} or {SwingClipPath}");
                return Report(u, scene);
            }

            ConfigureSwingImporter(u);
            AnimatorController controller = AddSwingToController(u);
            Material material = GetOrCreateMaterial(u);
            SetUpPlayer(u, controller, material);
            return Report(u, scene);
        }

        // ================================================================== Import + Animator

        /// <summary>
        /// The swing comes from a clip-only FBX (armature + one take, made by Tools/Blender/make_melee_swing.py).
        /// Generic rig with the witch's own avatar, so its bone curves bind to the witch's bones.
        /// </summary>
        private static void ConfigureSwingImporter(SetupUtils u)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(SwingClipPath);
            if (importer.clipAnimations.Any(c => c.name == SwingClip))
            {
                u.Skipped("swing clip import settings");
                return;
            }
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
            importer.animationType = ModelImporterAnimationType.Generic;
            if (avatar != null)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = avatar;
            }
            importer.optimizeGameObjects = false;
            importer.importAnimation = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;   // dense per-frame keys
            importer.materialImportMode = ModelImporterMaterialImportMode.None;      // armature only

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = SwingClip;   // the only take ("metarig|MeleeSwing")
                clip.loopTime = false;   // one-shot
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            u.Linked($"swing clip import: Generic, {(avatar != null ? "avatar copied from the witch" : "no avatar (curves bind by bone path)")}, {clips.Length} clip(s)");
        }

        private static AnimationClip LoadSwingClip() =>
            AssetDatabase.LoadAllAssetsAtPath(SwingClipPath).OfType<AnimationClip>()
                .FirstOrDefault(c => c.name == SwingClip);

        /// <summary>Adds the Attack trigger and a MeleeSwing one-shot state (from Any State, back to Locomotion).</summary>
        private static AnimatorController AddSwingToController(SetupUtils u)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                u.Warn($"{ControllerPath} not found");
                return null;
            }
            if (controller.parameters.All(p => p.name != AttackTrigger))
            {
                controller.AddParameter(AttackTrigger, AnimatorControllerParameterType.Trigger);
                u.Created($"AC_Player parameter {AttackTrigger}");
            }

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            if (sm.states.Any(s => s.state.name == SwingClip))
            {
                u.Skipped($"AC_Player state {SwingClip}");
                return controller;
            }
            AnimatorState locomotion = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == "Locomotion") ?? sm.defaultState;
            AnimatorState swing = sm.AddState(SwingClip);
            swing.motion = LoadSwingClip();

            // Enter from Any State as soon as Attack fires (quick 0.05 s blend, no exit time);
            // canTransitionToSelf = false: a click during the swing doesn't restart it.
            AnimatorStateTransition enter = sm.AddAnyStateTransition(swing);
            enter.AddCondition(AnimatorConditionMode.If, 0f, AttackTrigger);
            enter.hasExitTime = false;
            enter.duration = 0.05f;
            enter.canTransitionToSelf = false;

            // Back to Locomotion at 90 % of the clip, blending over 0.1 s.
            AnimatorStateTransition exit = swing.AddTransition(locomotion);
            exit.hasExitTime = true;
            exit.exitTime = 0.9f;
            exit.duration = 0.1f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            u.Created($"AC_Player state {SwingClip} (Any State —{AttackTrigger}→ {SwingClip} → Locomotion), clip: {(swing.motion != null ? swing.motion.name : "MISSING")}");
            return controller;
        }

        /// <summary>URP Lit material with the witch's base-colour texture (matte).</summary>
        private static Material GetOrCreateMaterial(SetupUtils u)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                u.Skipped(MaterialPath);
                return material;
            }
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_Witch" };
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            else u.Warn($"Texture not found: {TexturePath}");
            material.SetFloat("_Smoothness", 0.2f);
            AssetDatabase.CreateAsset(material, MaterialPath);
            u.Created(MaterialPath);
            return material;
        }

        // ================================================================== Scene

        private static void SetUpPlayer(SetupUtils u, AnimatorController controller, Material material)
        {
            PlayerMotor motor = Object.FindFirstObjectByType<PlayerMotor>();
            if (motor == null)
            {
                u.Warn("Player (PlayerMotor) not found: run Setup → Sprint 1 first");
                return;
            }
            Transform player = motor.transform;
            Transform model = player.Find("Model");
            if (model == null)
            {
                u.Warn("Player/Model not found");
                return;
            }

            // The witch as Model/Witch (a prefab instance of the FBX, so re-exports update the scene).
            Transform witch = model.Find("Witch");
            if (witch == null)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, model);
                Undo.RegisterCreatedObjectUndo(instance, "Setup");
                instance.name = "Witch";
                witch = instance.transform;
                witch.localPosition = Vector3.zero;          // feet at the player pivot
                witch.localRotation = Quaternion.identity;   // the witch faces +Z (checked in PA1)
                witch.localScale = Vector3.one * ModelScale;
                foreach (Renderer r in instance.GetComponentsInChildren<Renderer>())
                    r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
                u.Created($"Player/Model/Witch (scale {ModelScale}, M_Witch)");
            }
            else u.Skipped("Player/Model/Witch");

            Animator animator = u.GetOrAdd<Animator>(witch.gameObject);
            if (animator.runtimeAnimatorController == null && controller != null)
            {
                animator.runtimeAnimatorController = controller;
                u.Linked("Witch Animator → AC_Player");
            }
            animator.applyRootMotion = false;   // clips are in place; the CharacterController moves the player

            // Hide the greybox body and nose (visual-only children; the CharacterController stays).
            foreach (string greybox in new[] { "Body", "Nose" })
            {
                Transform t = model.Find(greybox);
                if (t == null || !t.gameObject.activeSelf) continue;
                Undo.RecordObject(t.gameObject, "Setup");
                t.gameObject.SetActive(false);
                u.Linked($"Player/Model/{greybox} hidden");
            }

            MoveSwordToHand(u, player, witch);

            var driver = u.GetOrAdd<PlayerAnimator>(player.gameObject);
            u.LinkIfEmpty(driver, "animator", animator);
            u.LinkIfEmpty(driver, "motor", motor);
            u.LinkIfEmpty(driver, "attack", player.GetComponent<PlayerAttack>());

            // The camera's wall check ignores the Player layer, so the witch must be on it too.
            if (witch.GetComponentsInChildren<Transform>(true).Any(t => t.gameObject.layer != player.gameObject.layer))
            {
                SetupUtils.SetLayerRecursively(player.gameObject, player.gameObject.layer);
                u.Linked("witch → Player layer");
            }
        }

        /// <summary>
        /// Parents the greybox sword to the witch's right-hand bone, so the swing clip carries it.
        /// Placed in the **Idle** pose (her bind pose lies on its back; the root-bone keys stand her up),
        /// sampled with AnimationMode so the bone changes are reverted afterwards: blade forward from the fist.
        /// The code-driven sweep (PlayerAttack.swingPivot) is then no longer needed.
        /// </summary>
        private static void MoveSwordToHand(SetupUtils u, Transform player, Transform witch)
        {
            Transform hand = witch.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "hand.R");
            Transform sword = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Sword");
            if (hand == null || sword == null)
            {
                u.Warn($"hand.R bone or Sword not found (hand: {hand != null}, sword: {sword != null})");
                return;
            }
            if (sword.parent == hand)
            {
                u.Skipped("Sword in hand.R");
                return;
            }

            AnimationClip idle = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().FirstOrDefault(c => c.name == "Idle");
            if (idle == null)
            {
                u.Warn("Idle clip not found; sword not moved");
                return;
            }

            Transform oldPivot = sword.parent;
            AnimationMode.StartAnimationMode();   // StopAnimationMode reverts the sampled bone poses
            try
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(witch.gameObject, idle, 0f);
                AnimationMode.EndSampling();
                Undo.SetTransformParent(sword, hand, "Setup");
                sword.SetPositionAndRotation(hand.position + player.forward * SwordGripOffset, player.rotation);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }
            u.Linked("Sword → witch hand.R bone (placed in the Idle pose, blade forward)");

            // The empty sweep pivot is replaced by the animation: unlink it and remove it.
            var attack = player.GetComponent<PlayerAttack>();
            if (attack != null && oldPivot != null && oldPivot.name == "SwordPivot" && oldPivot.childCount == 0)
            {
                SetupUtils.SetValue(attack, "swingPivot", p => p.objectReferenceValue = null);
                Undo.DestroyObjectImmediate(oldPivot.gameObject);
                u.Removed("Player/Model/SwordPivot (the swing clip moves the sword now)");
            }
        }

        private static string Report(SetupUtils u, Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Player character setup (Puzzle Master):\n" + u.Log + WiringValidator.Report();
        }
    }
}
