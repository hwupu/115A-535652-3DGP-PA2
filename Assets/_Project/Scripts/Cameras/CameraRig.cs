using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Cameras
{
    /// <summary>
    /// Third-person camera behind the player (ADR-0007, PB-03).
    /// Yaw: copied from the target (PlayerMotor turns the player with mouse X).
    /// Pitch: mouse Y, clamped. Distance: fixed, but a SphereCast pulls the camera in front of
    /// walls so it never looks through them. Put this on the Main Camera.
    /// Orbit (hold middle mouse or Left Alt; ported from PA1's orbit preview): the mouse circles the
    /// camera around the player without turning it, e.g. to show the character's front in the demo.
    /// On release the camera eases back behind the player. Only camera-side offsets change, so
    /// movement and attacks keep using the player's facing.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Tooltip("The player root to follow.")]
        [SerializeField] private Transform target;

        [Tooltip("Input source (Look, OrbitHold).")]
        [SerializeField] private GameInput input;

        [Header("Framing")]
        [Tooltip("Height of the orbit pivot above the target's feet (about head height).")]
        [SerializeField] private float pivotHeight = 1.6f;
        [Tooltip("Distance from the pivot to the camera when nothing is in the way.")]
        [SerializeField] private float distance = 5f;
        [Tooltip("Pitch at the start, in degrees (positive = looking down).")]
        [SerializeField] private float startPitch = 15f;
        [Tooltip("Lowest pitch (looking up), in degrees.")]
        [SerializeField] private float minPitch = -20f;
        [Tooltip("Highest pitch (looking down), in degrees.")]
        [SerializeField] private float maxPitch = 60f;

        [Header("Mouse")]
        [Tooltip("Degrees of pitch per pixel of mouse movement when no config.json is loaded.")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [Tooltip("Tick to invert the vertical mouse axis.")]
        [SerializeField] private bool invertY;

        [Header("Orbit (hold middle mouse / Left Alt)")]
        [Tooltip("Seconds the camera takes to return behind the player after the orbit button is released.")]
        [SerializeField, Min(0.01f)] private float orbitReturnTime = 0.35f;

        [Header("Wall avoidance")]
        [Tooltip("Radius of the sphere swept from the pivot to the camera.")]
        [SerializeField] private float collisionRadius = 0.25f;
        [Tooltip("Layers that block the camera. Exclude the Player layer.")]
        [SerializeField] private LayerMask collisionMask = ~0;

        private float pitch;
        private float orbitYaw, orbitPitch;                  // camera-only offsets while orbiting
        private float orbitYawVelocity, orbitPitchVelocity;  // SmoothDamp state for the return
        private readonly RaycastHit[] hits = new RaycastHit[8];

        private float Sensitivity => ConfigLoader.Current?.mouseSensitivity ?? mouseSensitivity;

        private void Awake()
        {
            pitch = startPitch;
            if (target == null) Debug.LogError($"{nameof(CameraRig)}: Target is not assigned; the camera won't follow.", this);
            if (input == null) Debug.LogError($"{nameof(CameraRig)}: Input is not assigned; mouse pitch won't work.", this);
        }

        // LateUpdate runs after every Update, so the player has already moved and turned this frame.
        private void LateUpdate()
        {
            if (target == null) return;

            if (input != null) UpdateLook();

            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            // The orbit offsets are added on top, so they never change the player's facing.
            Quaternion rotation = Quaternion.Euler(pitch + orbitPitch, target.eulerAngles.y + orbitYaw, 0f);
            Vector3 back = rotation * Vector3.back;

            transform.SetPositionAndRotation(pivot + back * ClearDistance(pivot, back), rotation);
        }

        /// <summary>
        /// Mouse Y tilts the camera. While orbiting, mouse X / Y move the orbit offsets instead
        /// (PlayerMotor stops turning); otherwise the offsets ease back to zero.
        /// </summary>
        private void UpdateLook()
        {
            Vector2 look = input.Look * Sensitivity;
            float dy = invertY ? look.y : -look.y;   // mouse up = look up = smaller pitch (unless inverted)

            if (input.IsOrbitHeld)
            {
                orbitYaw += look.x;   // unclamped: the camera can go all the way around
                // Clamp the combined pitch (base + offset), then keep only the offset part.
                orbitPitch = Mathf.Clamp(pitch + orbitPitch + dy, minPitch, maxPitch) - pitch;
                return;
            }

            pitch = Mathf.Clamp(pitch + dy, minPitch, maxPitch);
            // SmoothDamp is a critically damped spring (no overshoot); the Angle version
            // takes the short way around 360 degrees.
            orbitYaw = Mathf.SmoothDampAngle(orbitYaw, 0f, ref orbitYawVelocity, orbitReturnTime);
            orbitPitch = Mathf.SmoothDamp(orbitPitch, 0f, ref orbitPitchVelocity, orbitReturnTime);
        }

        /// <summary>How far the camera can go back from the pivot before a wall gets in the way.</summary>
        private float ClearDistance(Vector3 pivot, Vector3 back)
        {
            // NonAlloc: reuses the array instead of allocating every frame. Triggers never block the view.
            int count = Physics.SphereCastNonAlloc(pivot, collisionRadius, back, hits, distance,
                collisionMask, QueryTriggerInteraction.Ignore);
            float clear = distance;
            for (int i = 0; i < count; i++)
            {
                // Ignore the player's own collider (in case the mask still includes it).
                if (hits[i].transform.IsChildOf(target)) continue;
                clear = Mathf.Min(clear, hits[i].distance);
            }
            return clear;
        }
    }
}
