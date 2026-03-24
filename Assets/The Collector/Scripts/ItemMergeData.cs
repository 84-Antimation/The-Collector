using UnityEngine;
using System;
using System.Collections.Generic;

namespace FoxX.TheCollector
{
    [Serializable]
    public struct ItemMergeData
    {
        public string ItemName;
        public List<Item> Items;

        public ItemMergeData(Item firstItem)
        {
            ItemName = firstItem.name;

            Items = new List<Item>();
            Items.Add(firstItem);
        }

        public void Add(Item item)
        {
            Items.Add(item);
        }

        public bool CanMergeItems()
        {
            if (Items.Count < 3)
                return false;

            for (int i = 0; i < Items.Count; i++)
                if (!Items[i].IsOnSpot())
                    return false;

            int spotIndex0 = Items[0].Spot.transform.GetSiblingIndex();
            int spotIndex1 = Items[1].Spot.transform.GetSiblingIndex();
            int spotIndex2 = Items[2].Spot.transform.GetSiblingIndex();

            List<int> spotIndices = new List<int>();
            spotIndices.AddRange(new int[] { spotIndex0, spotIndex1, spotIndex2 });

            spotIndices.Sort();

            // Check if the items are on neighbor spots
            if (spotIndices[1] == spotIndices[0] + 1 && spotIndices[2] == spotIndices[0] + 2)
                return true;

            return false;
        }

        public void Remove(Item item) => Items.Remove(item);

        public Item Last() => Items[Items.Count - 1];

        public bool Contains(Item item)
            => Items.Contains(item);
    }
}