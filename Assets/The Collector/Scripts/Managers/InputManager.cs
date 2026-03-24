using UnityEngine;
using System;

namespace FoxX.TheCollector
{
    public class InputManager : MonoBehaviour
    {
        [Header(" Settings ")]
        private Item _currentItem;

        [Header(" Elements ")]
        [SerializeField] private Material _outlineMaterial;
        [SerializeField] private LayerMask _powerupLayer;

        [Header(" Actions ")]
        public static Action<Item> ItemClicked;
        public static Action<Powerup> PowerupClicked;

        // Update is called once per frame
        void Update()
        {
            if(GameManager.Instance.IsGame)
                HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
                HandleMouseDown();
            else if (Input.GetMouseButton(0))
                HandleDrag();
            else if (Input.GetMouseButtonUp(0))
                HandleMouseUp();
        }

        private void HandleMouseDown()
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, _powerupLayer);

            if (hit.collider == null)
                return;

            if (hit.collider.TryGetComponent(out Powerup powerup))
                PowerupClicked?.Invoke(powerup);
        }

        private void HandleDrag()
        {
            // Throw a rayast from the camera to screen pos and see if we can detect something
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100);

            if (hit.collider == null)
            {
                DeselectCurrentItem();
                return;
            }

            // The item selector / item is linked to the parent of the collider
            hit.collider.transform.parent.TryGetComponent(out Item item);

            // We did not hit an item
            if (item == null)
            {
                DeselectCurrentItem();
                return;
            }

            HandleItemClicked(item);
        }

        private void HandleItemClicked(Item item)
        {
            // Can this item be selected ? 
            if (!item.CanBeSelected())
                return;

            if (_currentItem != null)
                _currentItem.Deselect();

            _currentItem = item;
            _currentItem.Select(_outlineMaterial);
        }
        
        private void DeselectCurrentItem()
        {
            if (_currentItem != null)
                _currentItem.Deselect();

            _currentItem = null;
        }

        private void HandleMouseUp()
        {
            if (_currentItem == null)
                return;

            // We've selected an item
            ItemClicked?.Invoke(_currentItem);

            _currentItem.Deselect();
            _currentItem = null;
        }
    }
}