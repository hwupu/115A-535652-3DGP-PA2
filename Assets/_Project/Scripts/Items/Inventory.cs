using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleMaster.Items
{
    /// <summary>
    /// The items the player carries (PB-14): a count per <see cref="ItemDefinition"/>.
    /// PuzzleItem pickups call <see cref="Add"/>; puzzles (Sprint 3) call <see cref="TryRemove"/>;
    /// the HUD redraws on <see cref="Changed"/>. Lives on the Player.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        /// <summary>Raised after every add or remove.</summary>
        public event Action Changed;

        // Insertion order is kept by tracking the order separately, so the HUD lists items in pickup order.
        private readonly Dictionary<ItemDefinition, int> counts = new Dictionary<ItemDefinition, int>();
        private readonly List<ItemDefinition> order = new List<ItemDefinition>();

        /// <summary>Item kinds currently held (count > 0), in pickup order.</summary>
        public IEnumerable<ItemDefinition> Items
        {
            get
            {
                foreach (ItemDefinition item in order)
                    if (Count(item) > 0) yield return item;
            }
        }

        public int Count(ItemDefinition item) => item != null && counts.TryGetValue(item, out int n) ? n : 0;

        public void Add(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0) return;
            if (!counts.ContainsKey(item)) order.Add(item);
            counts[item] = Count(item) + amount;
            Debug.Log($"Inventory: +{amount} {item.DisplayName} (now {counts[item]})", this);
            Changed?.Invoke();
        }

        /// <summary>Removes <paramref name="amount"/> items if that many are held; returns false (and removes nothing) otherwise.</summary>
        public bool TryRemove(ItemDefinition item, int amount = 1)
        {
            if (Count(item) < amount || amount <= 0) return false;
            counts[item] -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
