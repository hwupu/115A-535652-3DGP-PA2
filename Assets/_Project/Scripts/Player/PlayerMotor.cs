using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Player
{
    /// <summary>
    /// Player movement and turning (ADR-0007, PB-02 / PB-03).
    /// - Mouse X turns the player (yaw). The CameraRig follows this yaw, so "forward" for WASD is
    ///   always the direction the camera looks.
    /// - WASD moves relative to that facing, with acceleration smoothing, on a CharacterController
    ///   (collides with walls and props without physics tuning). Gravity keeps the player grounded.
    /// Input comes from GameInput, which reads zero outside gameplay, so no state checks are needed.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Tooltip("Input source (Move, Look).")]
        [SerializeField] private GameInput input;

        [Header("Movement")]
        [Tooltip("Walking speed in m/s when no config.json is loaded.")]
        [SerializeField] private float moveSpeed = 5f;
        [Tooltip("How fast the speed changes, in m/s². Higher = snappier start and stop.")]
        [SerializeField] private float acceleration = 25f;
        [Tooltip("Downward acceleration in m/s².")]
        [SerializeField] private float gravity = 20f;

        [Header("Turning")]
        [Tooltip("Degrees of turn per pixel of mouse movement when no config.json is loaded.")]
        [SerializeField] private float mouseSensitivity = 0.15f;

        [Header("Safety")]
        [Tooltip("If the player falls below this height (e.g. through a gap), they are put back at the start.")]
        [SerializeField] private float fallResetHeight = -10f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float yaw;
        private Vector3 startPosition;
        private Quaternion startRotation;

        private float Speed => ConfigLoader.Current?.moveSpeed ?? moveSpeed;
        private float Sensitivity => ConfigLoader.Current?.mouseSensitivity ?? mouseSensitivity;

        /// <summary>Current horizontal speed in m/s (for animation later).</summary>
        public float CurrentSpeed => horizontalVelocity.magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
            startPosition = transform.position;
            startRotation = transform.rotation;
            if (input == null) Debug.LogError($"{nameof(PlayerMotor)}: Input is not assigned; the player can't move.", this);
        }

        private void Update()
        {
            if (input == null) return;
            Turn();
            Move();
            if (transform.position.y < fallResetHeight)
            {
                Debug.LogWarning($"Player fell below {fallResetHeight} m; moved back to the start.", this);
                ResetToStart();
            }
        }

        private void Turn()
        {
            // Mouse delta is already "pixels moved this frame", so it is not multiplied by deltaTime.
            yaw += input.Look.x * Sensitivity;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Move()
        {
            Vector2 wasd = input.Move;
            // Local to world: x = strafe along the player's right, y = along its forward.
            Vector3 wish = transform.right * wasd.x + transform.forward * wasd.y;
            wish = Vector3.ClampMagnitude(wish, 1f) * Speed;   // diagonal is not faster

            // Smooth start/stop: move the velocity toward the target by at most acceleration * dt.
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, wish, acceleration * Time.deltaTime);

            // A small constant push down keeps isGrounded reliable on flat ground.
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            else verticalVelocity -= gravity * Time.deltaTime;

            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        /// <summary>
        /// Puts the player back where the scene started. Used by the fall safety net, and by the
        /// optional respawn-at-0-HP demo toggle (Sprint 2, ADR-0008).
        /// </summary>
        public void ResetToStart()
        {
            // A CharacterController overrides transform changes while enabled, so disable it briefly.
            controller.enabled = false;
            transform.SetPositionAndRotation(startPosition, startRotation);
            controller.enabled = true;
            yaw = startRotation.eulerAngles.y;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
        }
    }
}
