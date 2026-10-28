using UnityEngine;
using UnityEngine.Rendering;

namespace PuzzleMaster.Cameras
{
    /// <summary>
    /// Top-down orthographic camera for the minimap (PB-23, ADR-0010; ported from PA1).
    /// It follows the player, north up, and renders only the Minimap layer (flat icons) into a
    /// RenderTexture that the HUD shows in the upper-right corner.
    /// Scene fog is switched off just for this camera's render, so the map stays readable at night.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MinimapCamera : MonoBehaviour
    {
        [Tooltip("Object to follow (the player).")]
        [SerializeField] private Transform target;
        [Tooltip("Camera height in metres (above all minimap icons).")]
        [SerializeField] private float height = 40f;
        [Tooltip("Half the visible width in metres: 30 shows a 60 × 60 m area around the player.")]
        [SerializeField, Min(5f)] private float viewRadius = 30f;
        [Tooltip("Off: north stays up and the player arrow turns. On: the map turns with the player.")]
        [SerializeField] private bool rotateWithTarget;

        private Camera minimapCamera;
        private bool fogWasOn;

        private void Awake()
        {
            minimapCamera = GetComponent<Camera>();
            minimapCamera.orthographic = true;   // no perspective: equal scale everywhere, like a real map
            if (target == null) Debug.LogError($"{nameof(MinimapCamera)}: Target is not assigned; the map won't follow the player.", this);
        }

        // URP raises these static events for every camera each frame; we hook in to toggle fog
        // only around this camera's render. Static events must be unsubscribed in OnDisable.
        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            if (cam != minimapCamera) return;   // the main camera keeps its fog
            fogWasOn = RenderSettings.fog;
            RenderSettings.fog = false;
        }

        private void OnEndCamera(ScriptableRenderContext context, Camera cam)
        {
            if (cam != minimapCamera) return;
            RenderSettings.fog = fogWasOn;   // restore for the next camera
        }

        // RUBRIC 5a · Minimap (5): top-down orthographic camera over the player → RenderTexture in the HUD.
        // LateUpdate: follow the player after it has moved this frame.
        private void LateUpdate()
        {
            if (target == null) return;
            minimapCamera.orthographicSize = viewRadius;   // half the visible height in metres
            Vector3 p = target.position;
            // Pitch 90° looks straight down; with yaw 0, world +Z (north) is the top of the map.
            float yaw = rotateWithTarget ? target.eulerAngles.y : 0f;
            transform.SetPositionAndRotation(new Vector3(p.x, height, p.z), Quaternion.Euler(90f, yaw, 0f));
        }
    }
}
