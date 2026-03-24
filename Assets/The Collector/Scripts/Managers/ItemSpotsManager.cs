using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

using Random = UnityEngine.Random;

namespace FoxX.TheCollector
{
    public class ItemSpotsManager : MonoBehaviour
    {
        public static ItemSpotsManager Instance;

        [Header(" Elements ")]
        [SerializeField] private Transform _itemSpotsParent;
        private ItemSpot[] _spots;

        [Header(" Data ")]
        private Dictionary<string, ItemMergeData> _itemToMergeData;

        [Header(" Settings ")]
        private bool _isBusy;

        [Header(" Actions ")]
        public static Action<Item> ItemPickedUp;
        public static Action ItemPlaced;
        public static Action<List<Item>> ItemsStartedMerge;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            InputManager.ItemClicked += OnItemClicked;
            PowerupManager.ItemBackToGame += OnItemBackToGame;

            StoreSpots();
        }

        private void OnDestroy()
        {
            InputManager.ItemClicked -= OnItemClicked;
            PowerupManager.ItemBackToGame -= OnItemBackToGame;
        }

        private void OnItemClicked(Item item)
        {
            if (_isBusy)
            {
                Debug.LogWarning("ItemSpotsManager: busy");
                return;
            }

            // Do we have a free spot ?
            if (!FreeSpotAvailable())
            {
                Debug.LogWarning("ItemSpotsManager: No free spot available");
                return;
            }

            // There is stuff to be done !
            _isBusy = true;

            ItemPickedUp?.Invoke(item);

            HandleItemClicked(item);
        }

        private void OnItemBackToGame(Item item)
        {
            if (!_itemToMergeData.ContainsKey(item.name))
                return;

            _itemToMergeData[item.name].Remove(item);

            if (_itemToMergeData[item.name].Items.Count <= 0)
                _itemToMergeData.Remove(item.name);
        }
        
        private void HandleItemClicked(Item item)
        {
            // Do we have any merge data for that item ?
            if (_itemToMergeData.ContainsKey(item.name))
                HandleItemMergeDataFound(item);
            else
                TryMoveItemToFirstFreeSpot(item);            
        }

        private void HandleItemMergeDataFound(Item item)
        {
            // Try and get the ideal spot 
            ItemSpot idealSpot = GetIdealSpotFor(item);

            if (idealSpot == null)
            {
                Debug.LogWarning("ItemSpotsManager: Ideal spot is null...\nThis should not happen");
                return;
            }
            // Add item to merge data
            _itemToMergeData[item.name].Add(item);

            // Try and move the item to that ideal spot
            TryMoveItemToIdealSpot(item, idealSpot);
        }

        private void TryMoveItemToIdealSpot(Item item, ItemSpot spot)
        {
            // Is there something at that spot ?
            if (!spot.IsEmpty())
            {
                HandleIdealSpotFull(item, spot);
                return;
            }
            // The spot is empty, move the item there
            MoveItemToSpot(item, spot, () => HandleItemReachedSpot(item));
        }

        private void HandleItemReachedSpot(Item item, bool checkForMerge = true)
        {
            // Animate the spot
            item.Spot.BumpDown();
            item.SetState(EItemState.OnSpot);

            if (!checkForMerge)
                return;

            // Is there a merge ?
            if (_itemToMergeData[item.name].CanMergeItems())
                MergeItemsFrom(_itemToMergeData[item.name]);
            else
                CheckForGameover();
        }

        private void MergeItemsFrom(ItemMergeData mergeData)
        {
            // Grab the items from the merge data
            List<Item> items = mergeData.Items;

            // Delete the merge data
            _itemToMergeData.Remove(mergeData.ItemName);

            for (int i = items.Count - 1; i >= 0; i--)
            {
                // Unpopulate the spot
                items[i].SetState(EItemState.Merging);
                items[i].Spot.Clear();
            }

            // Let's try and move the items right away to save time
            OnMergeComplete();
            ItemsStartedMerge?.Invoke(items);
        }

        private void OnMergeComplete()
        {
            // Do we have any item left ?
            if (_itemToMergeData.Count <= 0)
            {
                Debug.Log("ItemSpotsManager: No more items in the spots, not busy anymore");
                _isBusy = false;
            }
            else
                MoveAllItemsToTheLeft(HandleAllItemsMovedToTheLeft);
        }

        private void MoveAllItemsToTheLeft(Action completeCallback)
        {
            // Start from 1 cause we can't move item at spot 0 to the left lol
            bool callbackTriggered = false;

            for (int i = 3; i < _spots.Length; i++)
            {
                ItemSpot spot = _spots[i];

                if (spot.IsEmpty())
                    continue;

                // Cache the item before clearing
                Item item = spot.Item;

                if (!item.IsOnSpot())
                    continue;

                ItemSpot targetSpot = _spots[i - 3];

                if (!targetSpot.IsEmpty())
                {
                    Debug.LogWarning($"{targetSpot.name} is full, can't move anything there");
                    _isBusy = false;
                    return;
                }

                // Clear the spot
                spot.Clear();

                // Only trigger the callback when the spot for our target item is freed
                if (!callbackTriggered)
                {
                    completeCallback += () => HandleItemReachedSpot(item, false);
                    MoveItemToSpot(item, targetSpot, completeCallback);
                    callbackTriggered = true;
                }
                else
                    MoveItemToSpot(item, targetSpot, () => HandleItemReachedSpot(item, false));
            }

            // In case we haven't move any item
            if(!callbackTriggered)
            {
                completeCallback?.Invoke();
            }

        }

        private void HandleAllItemsMovedToTheLeft()
        {
            _isBusy = false;
        }

        private void HandleIdealSpotFull(Item item, ItemSpot spot)
        {
            // Move all the items on this spot + on the right, to the right
            MoveAllItemsToTheRightFrom(spot, item, null);
        }

        private void MoveAllItemsToTheRightFrom(ItemSpot startSpot, Item itemToPlace, Action completeCallback)
        {
            int spotIndex = startSpot.transform.GetSiblingIndex();

            for (int i = _spots.Length - 2; i >= spotIndex; i--)
            {
                ItemSpot spot = _spots[i];

                if (spot.IsEmpty())
                    continue;

                // Cache the item before clearing
                Item item = spot.Item;

                if (!item.IsOnSpot())
                    continue;

                // Clear the spot
                spot.Clear();

                ItemSpot targetSpot = _spots[i + 1];

                if(!targetSpot.IsEmpty())
                {
                    Debug.LogWarning($"{targetSpot.name} is full, can't move anything there\nThis should not happen");
                    _isBusy = false;
                    return;
                }

                MoveItemToSpot(item, targetSpot, () => HandleItemReachedSpot(item, false));
            }

            MoveItemToSpot(itemToPlace, startSpot, () => HandleItemReachedSpot(itemToPlace));
        }

        private ItemSpot GetIdealSpotFor(Item item)
        {
            // So first off, we are going to check in the itemMergeData
            // And get the far right item 

            List<Item> items = _itemToMergeData[item.name].Items;
            List<ItemSpot> itemSpots = new List<ItemSpot>();

            for (int i = 0; i < items.Count; i++)
                itemSpots.Add(items[i].Spot);            

            if(itemSpots.Count <= 0)
            {
                Debug.LogError("ItemSpotsManager: No spots found... This should not happen");
                return null;
            }

            // Now that we have the spots
            if(itemSpots.Count >= 2)
                itemSpots.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

            // Let's now grab the spot on the right hand side of this one
            int idealSpotIndex = itemSpots[0].transform.GetSiblingIndex() + 1;

            if(idealSpotIndex >= _spots.Length)
            {
                Debug.LogError("ItemSpotsManager: Spot index is out of range...\nThis should not happen");
                return null;
            }

            return _spots[idealSpotIndex];
        }

        private void CreateItemMergeData(Item item)
        {
            _itemToMergeData.Add(item.name, new ItemMergeData(item));
        }

        private void TryMoveItemToFirstFreeSpot(Item item)
        {
            ItemSpot targetSpot = GetFreeSpot();

            if (targetSpot == null)
            {
                Debug.LogError("ItemSpotsManager: No free spot found, this should not happen");
                _isBusy = false;
                return;
            }

            // A free spot has been found
            // Create the merge data
            CreateItemMergeData(item);

            // Move the item there
            MoveItemToSpot(item, targetSpot, () => HandleFirstItemReachedSpot(item));
        }

        private void MoveItemToSpot(Item item, ItemSpot spot, Action completeCallback)
        {
            // This will turn the item into a child of the spot
            item.SetState(EItemState.MovingToSpot);
            spot.Populate(item);

            // Lock any item physics
            item.DisablePhysics();
            item.DisableShadows();

            // Now we can use LeanTween to move the item to that spot in a cool fashion
            float animationDuration = .15f;
            LeanTween.moveLocal(item.gameObject, Vector3.up * .01f, animationDuration);
            LeanTween.rotateLocal(item.gameObject, Vector3.zero, animationDuration);
            LeanTween.scale(item.gameObject, Vector3.one * .2f, animationDuration)
                .setOnComplete(completeCallback);
        }

        private void HandleFirstItemReachedSpot(Item item)
        {
            // Animate the spot
            item.Spot.BumpDown();
            item.SetState(EItemState.OnSpot);

            // Check for gameover
            CheckForGameover();

            //isBusy = false;
        }

        private void CheckForGameover()
        {
            if (GetFreeSpot() == null)
                GameManager.Instance.SetGameState(EGameState.GAMEOVER);
            else
                _isBusy = false;
        }

        private void StoreSpots()
        {
            _spots = new ItemSpot[_itemSpotsParent.childCount];

            for (int i = 0; i < _itemSpotsParent.childCount; i++)
                _spots[i] = _itemSpotsParent.GetChild(i).GetComponent<ItemSpot>();

            Debug.Log($"ItemSpotsManager: Found {_spots.Length} spots");
        }

        private ItemSpot GetFreeSpot()
        {
            for (int i = 0; i < _spots.Length; i++)
                if (_spots[i].IsEmpty())
                    return _spots[i];

            return null;
        }

        private bool FreeSpotAvailable()
        {
            for (int i = 0; i < _spots.Length; i++)
                if (_spots[i].IsEmpty())
                    return true;

            return false;
        }

        public ItemSpot GetRandomOccupiedSpot()
        {
            List<ItemSpot> occupiedSpots = new List<ItemSpot>();

            for (int i = 0; i < _spots.Length; i++)
            {
                if (_spots[i].IsEmpty())
                    continue;

                occupiedSpots.Add(_spots[i]);
            }

            if (occupiedSpots.Count <= 0)
                return null;
            
            return occupiedSpots[Random.Range(0, occupiedSpots.Count)];
        }
    }
}