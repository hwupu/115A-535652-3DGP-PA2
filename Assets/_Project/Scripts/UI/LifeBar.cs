using PuzzleMaster.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// Life bar above a monster's head (PB-12): a small world-space canvas that always faces the
    /// camera. The fill's width follows Health.Normalized (scaled from its left-edge pivot,
    /// which works without a sprite), and its colour goes from green (full) to red (low).
    /// Hidden once the monster is recovered.
    /// </summary>
    public class LifeBar : MonoBehaviour
    {
        [Tooltip("The Health to display (usually on the monster root).")]
        [SerializeField] private Health health;
        [Tooltip("The fill bar: its pivot must be at the left edge (x = 0).")]
        [SerializeField] private RectTransform fill;
        [Tooltip("The fill's Image, for the colour.")]
        [SerializeField] private Image fillImage;

        [SerializeField] private Color fullColor = new Color(0.3f, 0.9f, 0.3f);
        [SerializeField] private Color lowColor = new Color(0.95f, 0.2f, 0.2f);

        private Transform cameraTransform;

        private void OnEnable()
        {
            if (health == null)
            {
                Debug.LogError($"{nameof(LifeBar)} on {name}: Health is not assigned.", this);
                return;
            }
            health.Changed += Redraw;
            health.Depleted += Hide;
            Redraw(0f);
        }

        private void OnDisable()
        {
            if (health == null) return;
            health.Changed -= Redraw;
            health.Depleted -= Hide;
        }

        // LateUpdate: after the camera has moved this frame. Copying the camera's rotation keeps
        // the bar parallel to the screen (no skew at the screen edges, unlike LookAt).
        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                Camera main = Camera.main;
                if (main == null) return;
                cameraTransform = main.transform;
            }
            transform.rotation = cameraTransform.rotation;
        }

        private void Redraw(float _)
        {
            float value = health.Normalized;
            if (fill != null) fill.localScale = new Vector3(value, 1f, 1f);
            if (fillImage != null) fillImage.color = Color.Lerp(lowColor, fullColor, value);
        }

        private void Hide() => gameObject.SetActive(false);
    }
}
