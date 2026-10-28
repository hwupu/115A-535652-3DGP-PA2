using PuzzleMaster.Core;
using PuzzleMaster.Items;
using UnityEngine;

namespace PuzzleMaster.Puzzles
{
    /// <summary>
    /// A puzzle that needs n of one item (ADR-0009): the key door (Key ×1, PB-18), the crystal altar
    /// (Crystal ×3, PB-19) and the artifact pedestal (Artifact ×1, PB-21).
    /// Items are used **automatically** (PO decision): when the player enters the trigger zone, every
    /// matching item they carry is taken (up to what is still needed). Without one, a hint says what to
    /// bring. Each placed item appears on a slot as a copy of its pickup's model, so a new item model
    /// (e.g. the PO's artifact) shows up here too without extra work.
    /// Needs a trigger collider and a kinematic Rigidbody on this object (set up by Setup → Sprint 3).
    /// </summary>
    public class ItemSocketPuzzle : Puzzle
    {
        [Tooltip("The item this puzzle needs.")]
        [SerializeField] private ItemDefinition item;
        [Tooltip("How many of the item are needed.")]
        [SerializeField, Min(1)] private int required = 1;
        [Tooltip("Where placed items appear, one per item, in order. Missing slots show nothing.")]
        [SerializeField] private Transform[] slots = new Transform[0];
        [Tooltip("Slide the opening (e.g. the door) away when solved.")]
        [SerializeField] private bool openWhenSolved;

        private int placed;

        private string ItemName => item != null ? item.DisplayName : "?";

        public override string Status =>
            IsSolved ? "solved"
            : required == 1 ? $"bring the {ItemName}"
            : $"{ItemName}s {placed}/{required}";

        private void Awake()
        {
            if (item == null) Debug.LogError($"{name}: {nameof(ItemSocketPuzzle)} has no item; it can't be solved.", this);
        }

        // RUBRIC 2a · Five puzzles (15): puzzles 1, 2, 4 (Key door, Crystal altar ×3, Artifact pedestal). Items are used to solve.
        private void OnTriggerEnter(Collider other)
        {
            if (IsSolved || item == null) return;
            Inventory inventory = PlayerInventory(other);
            if (inventory == null) return;

            // Take what the player has, but never more than the puzzle still needs.
            int take = Mathf.Min(inventory.Count(item), required - placed);
            if (take <= 0)
            {
                GameEvents.RaisePuzzleMessage(required == 1
                    ? $"{Title}: you need the {ItemName}."
                    : $"{Title}: {placed}/{required} placed. Find more {ItemName}s.");
                return;
            }

            inventory.TryRemove(item, take);
            for (int i = 0; i < take; i++) ShowInSlot(placed++);
            GameEvents.RaisePuzzleProgressed(transform.position);

            if (placed >= required) MarkSolved();
            else
            {
                GameEvents.RaisePuzzleMessage($"{Title}: {placed}/{required} {ItemName}s placed.");
                NotifyChanged();
            }
        }

        protected override void OnSolved()
        {
            if (openWhenSolved) Open();
        }

        /// <summary>Puts a copy of the item's pickup model on slot <paramref name="index"/> (visual only).</summary>
        private void ShowInSlot(int index)
        {
            if (slots == null || index >= slots.Length || slots[index] == null) return;
            Transform visual = item.PickupPrefab != null ? item.PickupPrefab.Visual : null;
            if (visual == null) return;

            GameObject copy = Instantiate(visual.gameObject, slots[index], false);
            copy.transform.localPosition = Vector3.zero;
            // A copy is decoration: remove any colliders the model brought along.
            foreach (Collider c in copy.GetComponentsInChildren<Collider>()) Destroy(c);
        }
    }
}
