# Builds the witch's "MeleeSwing" clip (ADR-0012) on a COPY of "The little Witch.blend" (PA1 source),
# renders preview frames, and exports a clip-only FBX for Unity.
#
#   cp "<PA1 Assets>/the-little-witch/source/The little Witch.blend" /tmp/witch/witch.blend
#   ~/Applications/Blender.app/Contents/MacOS/Blender -b /tmp/witch/witch.blend --python Tools/Blender/make_melee_swing.py
#   cp "/tmp/witch/out/The little Witch@MeleeSwing.fbx" "Assets/_Project/Models/Cute Witch/"
#
# How the clip is made: the upper body (spine.001 and up, arms) is sampled from the existing
# right-handed sidearm "ThrowBoomerang" action, played ~3x faster (36 -> 12 frames @ 24 fps = 0.46 s,
# about the 0.5 s attack cooldown); the pelvis, legs and skirt are held at Idle frame 1, so the feet stay
# planted. All 27 keyed bones are keyed every frame, like the PA1 clips. Export settings match the PA1
# FBX (Forward -Z, Up Y, no leaf bones, primary bone axis Y). Apply Scalings must stay at Blender's default
# "All Local" (FBX_SCALE_NONE): the PA1 FBX was exported that way, and "FBX Units Scale" (as the PA1 handoff
# said) makes every bone position 100x too large in Unity. Blender 5 (layered actions).
import bpy, os
OUT = "/tmp/witch/out"; os.makedirs(OUT, exist_ok=True)
scene = bpy.context.scene
arm = bpy.data.objects["metarig"]
throw, idle = bpy.data.actions["ThrowBoomerang"], bpy.data.actions["Idle"]

def keyed_bones(action):
    names = set()
    for layer in getattr(action, "layers", []):
        for strip in layer.strips:
            for cb in strip.channelbags:
                for fc in cb.fcurves:
                    if '"' in fc.data_path: names.add(fc.data_path.split('"')[1])
    for fc in getattr(action, "fcurves", []) or []:
        if '"' in fc.data_path: names.add(fc.data_path.split('"')[1])
    return sorted(names)

bones = keyed_bones(throw)
print("KEYED", len(bones), bones)
UPPER = {"spine.001", "spine.002", "spine.003", "spine.004", "spine.005", "spine.006",
         "shoulder.L", "shoulder.R", "upper_arm.L", "upper_arm.R", "forearm.L", "forearm.R", "hand.L", "hand.R"}

def use(action):
    arm.animation_data.action = action
    if hasattr(arm.animation_data, "action_slot") and arm.animation_data.action_slot is None and len(action.slots):
        arm.animation_data.action_slot = action.slots[0]

def sample(action, frame):
    use(action)
    whole = int(frame); scene.frame_set(whole, subframe=frame - whole)
    return {b: (arm.pose.bones[b].location.copy(), arm.pose.bones[b].rotation_quaternion.copy()) for b in bones}

N = 12                                   # 12 frames @ 24 fps = 0.458 s
idle_pose = sample(idle, 1)
throw_len = throw.frame_range[1] - throw.frame_range[0]
poses = [sample(throw, throw.frame_range[0] + i * throw_len / (N - 1)) for i in range(N)]

swing = bpy.data.actions.new("MeleeSwing")
use(swing)
for i, pose in enumerate(poses):
    for b in bones:
        loc, rot = (pose if b in UPPER else idle_pose)[b]
        pb = arm.pose.bones[b]
        pb.location, pb.rotation_quaternion = loc, rot
        pb.keyframe_insert("rotation_quaternion", frame=i + 1, group=b)
        if b == "spine": pb.keyframe_insert("location", frame=i + 1, group=b)
swing.use_frame_range = True
swing.frame_start, swing.frame_end = 1, N
swing.use_fake_user = True
print("SWING keyed bones", len(keyed_bones(swing)), "range", tuple(swing.frame_range))

# Preview renders (Workbench, camera front-right; the witch faces -Y in Blender)
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.resolution_x = scene.render.resolution_y = 360
scene.render.film_transparent = False
cam_data = bpy.data.cameras.new("PreviewCam"); cam = bpy.data.objects.new("PreviewCam", cam_data)
scene.collection.objects.link(cam); scene.camera = cam
cam.location = (1.6, -2.4, 0.9)
target = bpy.data.objects.new("Target", None); scene.collection.objects.link(target); target.location = (0, 0, 0.55)
c = cam.constraints.new("TRACK_TO"); c.target = target; c.track_axis = "TRACK_NEGATIVE_Z"; c.up_axis = "UP_Y"
for f in (1, 4, 6, 8, 12):
    scene.frame_set(f); scene.render.filepath = f"{OUT}/swing_{f:02d}.png"; bpy.ops.render.render(write_still=True)
bpy.data.objects.remove(cam); bpy.data.objects.remove(target)
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/witch_with_swing.blend", copy=True)

# Clip-only FBX: armature + MeleeSwing only (other actions removed in this copy)
for a in list(bpy.data.actions):
    if a.name != "MeleeSwing": bpy.data.actions.remove(a)
use(swing)
# An extra root-level empty keeps "metarig" as a child of the FBX root in Unity. With the armature
# alone, Unity collapses it into the root and the curve paths become "spine/..." instead of
# "metarig/spine/...", so the clip would not bind to the witch's bones.
keep = bpy.data.objects.new("ClipRoot", None); scene.collection.objects.link(keep)
for o in bpy.context.view_layer.objects: o.select_set(o in (arm, keep))
bpy.context.view_layer.objects.active = arm
bpy.ops.export_scene.fbx(filepath=f"{OUT}/The little Witch@MeleeSwing.fbx", use_selection=True, object_types={"ARMATURE", "EMPTY"},
    global_scale=1.0, apply_scale_options="FBX_SCALE_NONE", axis_forward="-Z", axis_up="Y",
    add_leaf_bones=False, primary_bone_axis="Y", secondary_bone_axis="X", use_armature_deform_only=False,
    armature_nodetype="NULL", bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0)
print("EXPORTED", os.path.getsize(f"{OUT}/The little Witch@MeleeSwing.fbx"))
