using System.Text;
using PuzzleMaster.Items;
using TMPro;
using UnityEngine;

namespace PuzzleMaster.UI
{
    /// <summary>
    /// Lists the items the player holds (PB-14), e.g. "Key ×1   Crystal ×2", each in its
    /// item colour (TextMeshPro rich-text colour tags). Redraws only when the inventory changes.
    /// </summary>
    public class InventoryHud : MonoBehaviour
    {
        [Tooltip("The player's inventory.")]
        [SerializeField] private Inventory inventory;
        [Tooltip("Text that lists the items.")]
        [SerializeField] private TMP_Text text;
        [Tooltip("Shown when no items are held.")]
        [SerializeField] private string emptyText = "Items: none";

        private readonly StringBuilder builder = new StringBuilder();

        private void OnEnable()
        {
            if (inventory == null)
            {
                Debug.LogError($"{nameof(InventoryHud)}: Inventory is not assigned.", this);
                return;
            }
            inventory.Changed += Redraw;
            Redraw();
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.Changed -= Redraw;
        }

        private void Redraw()
        {
            if (text == null) return;
            builder.Clear().Append("Items: ");
            bool any = false;
            foreach (ItemDefinition item in inventory.Items)
            {
                if (any) builder.Append("   ");
                builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(item.Color)).Append('>')
                       .Append(item.DisplayName).Append(" ×").Append(inventory.Count(item)).Append("</color>");
                any = true;
            }
            text.text = any ? builder.ToString() : emptyText;
        }
    }
}
