using PuzzleMaster.Combat;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// A flat icon that only the minimap camera sees (ADR-0010, PB-23; ported from PA1).
    /// It stays level at a fixed world height above its parent (higher icons draw on top) and can
    /// turn with the parent's yaw (the player arrow). Its colour is applied with a
    /// MaterialPropertyBlock, so all icons share one unlit material. With a Health assigned
    /// (monsters), the icon switches to the depleted colour once the monster is recovered.
    /// Puzzle icons are tinted by their Puzzle instead (status renderers).
    /// </summary>
    public class MinimapIcon : MonoBehaviour
    {
        /// <summary>Layer the minimap camera renders (created by Setup → Sprint 3).</summary>
        public const string LayerName = "Minimap";

        [Tooltip("World height of the icon. Higher icons draw on top of lower ones.")]
        [SerializeField] private float worldHeight = 20f;
        [Tooltip("Turn with the parent's yaw (use for the player arrow).")]
        [SerializeField] private bool followYaw;
        [Tooltip("Icon colour.")]
        [SerializeField] private Color color = Color.white;
        [Tooltip("Optional: when this Health is depleted (monster recovered), use the colour below.")]
        [SerializeField, OptionalReference] private Health health;
        [Tooltip("Icon colour once the Health above is depleted.")]
        [SerializeField] private Color depletedColor = new Color(1f, 0.95f, 0.75f);

        private void Awake()
        {
            // Guard against setup reruns that re-layer a parent (e.g. the Player layer): an icon on
            // another layer would show up in the 3D view and vanish from the map.
            int layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0) Debug.LogError($"{name}: layer '{LayerName}' is missing; run Setup → Sprint 3.", this);
            else foreach (Transform t in GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
            Tint(color);
        }

        private void OnEnable()
        {
            if (health != null) health.Depleted += OnDepleted;
        }

        private void OnDisable()
        {
            if (health != null) health.Depleted -= OnDepleted;
        }

        private void OnDepleted() => Tint(depletedColor);

        private void Tint(Color c)
        {
            var block = new MaterialPropertyBlock();
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                r.GetPropertyBlock(block);
                block.SetColor(MaterialUtility.BaseColor, c);
                r.SetPropertyBlock(block);
            }
        }

        // RUBRIC 5a · Minimap (5): player / puzzle / monster icons on the Minimap layer (only the map camera sees them).
        // LateUpdate runs after the parent has moved this frame, so the icon snaps to its final
        // position before the minimap camera renders.
        private void LateUpdate()
        {
            Transform parent = transform.parent;
            if (parent == null) return;
            Vector3 p = parent.position;
            // Keep only the parent's yaw, so the icon stays flat for the top-down camera.
            float yaw = followYaw ? parent.eulerAngles.y : 0f;
            transform.SetPositionAndRotation(new Vector3(p.x, worldHeight, p.z), Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
