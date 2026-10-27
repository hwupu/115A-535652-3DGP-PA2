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
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 visualStart;
        private bool collected;

        public ItemDefinition Item => item;

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
