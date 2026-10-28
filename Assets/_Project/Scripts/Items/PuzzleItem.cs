using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Items
{
    /// <summary>
    /// A puzzle item lying in the world (PB-14). Walking into it puts it into the player's
    /// Inventory automatically (PO decision: no interact key), then it disappears.
    /// The visual child bobs and spins so drops are easy to spot. Root: trigger collider +
    /// kinematic Rigidbody (trigger events with the CharacterController need a Rigidbody).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PuzzleItem : MonoBehaviour
    {
        [Tooltip("What this pickup is.")]
        [SerializeField] private ItemDefinition item;
        [Tooltip("The visual child that bobs and spins.")]
        [SerializeField] private Transform visual;

        [Header("Idle motion")]
        [Tooltip("Spin around the vertical axis, in degrees per second.")]
        [SerializeField] private float spinSpeed = 90f;
        [Tooltip("How far the visual bobs up and down, in metres.")]
        [SerializeField] private float bobHeight = 0.15f;
        [Tooltip("Bob speed (radians per second of the sine wave).")]
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 visualStart;
        private bool collected;

        public ItemDefinition Item => item;
        /// <summary>The visual child (Model). Puzzles copy it to show a placed item (ADR-0009).</summary>
        public Transform Visual => visual;

        private void Awake()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            if (visual != null) visualStart = visual.localPosition;
            else Debug.LogWarning($"{name}: PuzzleItem.visual is empty; the item won't bob.", this);
            if (item == null) Debug.LogError($"{name}: PuzzleItem has no ItemDefinition.", this);
        }

        private void Update()
        {
            if (visual == null) return;
            visual.localPosition = visualStart + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
            visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        // RUBRIC 1c · Item collection (10): walking into the dropped item puts it into the player's Inventory.
        // Fires because the item's collider is a trigger and it has a (kinematic) Rigidbody;
        // the player's CharacterController counts as the other collider.
        private void OnTriggerEnter(Collider other)
        {
            if (collected || item == null) return;
            var inventory = other.GetComponentInParent<Inventory>();
            if (inventory == null) return;   // only the player carries an Inventory

            collected = true;   // guard: several colliders may enter in the same frame
            inventory.Add(item);
            GameEvents.RaiseItemCollected(item.DisplayName);
            Destroy(gameObject);
        }
    }
}
