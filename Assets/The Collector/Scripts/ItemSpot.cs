using UnityEngine;

namespace FoxX.TheCollector
{
    public class ItemSpot : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _itemParent;

        [Header(" Settings ")]
        private Item _item;
        public Item Item => _item;

        public void Populate(Item item)
        {
            _item = item;
            item.AssignSpot(this);

            // The item is now a child of this spot, and it is the first child
            item.transform.SetParent(_itemParent);
        }

        public void BumpDown()
        {
            _animator.Play("BumpDown", 0, 0);
        }

        public void Clear()
        {
            _item = null;
        }

        public bool IsEmpty()
        {
            return _item == null;
        }
    }
}