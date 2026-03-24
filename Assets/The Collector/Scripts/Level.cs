using UnityEngine;

namespace FoxX.TheCollector
{
    public class Level : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private ItemPlacer _itemPlacer;
        public Transform ItemsParent => _itemPlacer.transform;

        [Header(" Settings ")]
        [SerializeField] private int _duration;
        public int Duration => _duration;


        public ItemLevelData[] GetGoals()
            => _itemPlacer.GetGoals();

        public Item[] GetItems()
            => _itemPlacer.GetItems();
    }
}