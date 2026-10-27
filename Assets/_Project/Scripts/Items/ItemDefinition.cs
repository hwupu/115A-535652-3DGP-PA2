using UnityEngine;

namespace PuzzleMaster.Items
{
    /// <summary>
    /// One kind of puzzle item (ADR-0009), e.g. Key, Crystal, Artifact. A data asset, so new item
    /// kinds and theme renames need no code: create one via Assets → Create → Puzzle Master → Item.
    /// Monsters drop <see cref="PickupPrefab"/>; the inventory counts items per definition;
    /// puzzles ask for "n of this definition".
    /// </summary>
    [CreateAssetMenu(fileName = "Item_New", menuName = "Puzzle Master/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("Name shown in the HUD and messages.")]
        [SerializeField] private string displayName = "Item";
        [Tooltip("Colour for the HUD text and the minimap icon.")]
        [SerializeField] private Color color = Color.white;
        [Tooltip("The pickup prefab a monster drops for this item.")]
        [SerializeField] private PuzzleItem pickupPrefab;

        public string DisplayName => displayName;
        public Color Color => color;
        public PuzzleItem PickupPrefab => pickupPrefab;
    }
}
