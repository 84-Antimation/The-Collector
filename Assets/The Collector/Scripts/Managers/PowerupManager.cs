using NaughtyAttributes;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;

namespace FoxX.TheCollector
{
    public class PowerupManager : MonoBehaviour
    {
        public static PowerupManager Instance;

        [Header(" Actions ")]
        public static Action<Item> ItemPickedUp;
        public static Action<Item> ItemBackToGame;
        public static Action<List<Item>> ItemsSelected;

        [Header(" Settings ")]
        [SerializeField] private float _fanMagnitude;
        private bool _isUsingPowerup;

        [Header(" Vacuum Settings ")]
        [SerializeField] private Animator _vacuumAnimator; 
        [SerializeField] private Transform _vacuumSuckPosition;
        [SerializeField] private AnimationCurve _vacItemYCurve;

        [Header(" Vacuum Elements ")]
        [SerializeField] private TextMeshPro _vacuumAmountText;
        [SerializeField] private GameObject _vacuumVideoIcon;

        [Header(" General PU Settings ")]
        [SerializeField] private int _initialVacuumPUCount;
        private int _vacuumPUCount;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            LoadData();

            Vacuum.started += OnVacuumStarted;
            InputManager.PowerupClicked += OnPowerupClicked;
        }

        private void OnDestroy()
        {
            Vacuum.started -= OnVacuumStarted;
            InputManager.PowerupClicked -= OnPowerupClicked;
        }

        private void OnPowerupClicked(Powerup clickedPowerup)
        {
            if (_isUsingPowerup)
                return;

            int amount = 0;

            switch (clickedPowerup.PowerupType)
            {
                case EPowerupType.Vacuum:
                    VacuumPowerupPressed();
                    amount = _vacuumPUCount;
                    break;
            }

            clickedPowerup.UpdateVisuals(amount);
        }

        private void OnVacuumStarted()
        {
            StartCoroutine(VacuumCoroutine());            
        }

        IEnumerator VacuumCoroutine()
        {
            // Grab all items & current goal states
            Item[] items = LevelManager.Instance.Items;
            ItemLevelData[] goals = GoalManager.Instance.Goals;

            // From those goals, grab the first one that has at least 3 elements
            // If none of them has 3 elements, grab the one that has the most

            // Or we can simply grab the one that has the most amounts after all 
            ItemLevelData greatestGoal = GetGreatestGoal(goals);

            // Now that we have the greatest goal
            // We want to grab at max three of those items
            List<Item> itemsToCollect = new List<Item>();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].name == greatestGoal.ItemPrefab.name)
                {
                    itemsToCollect.Add(items[i]);

                    if (itemsToCollect.Count >= 3)
                        break;
                }
            }

            Vector3 finalTargetPosition = _vacuumSuckPosition.position;

            int collectedItems = 0;
            float timer = 0;
            float animationDuration = .7f;
            float delayBetweenItems = .3f;

            LTBezierPath[] itemPaths = new LTBezierPath[3];

            // Store the Beziers of the three items
            for (int i = 0; i < itemsToCollect.Count; i++)
            {
                itemPaths[i] = new LTBezierPath();

                Vector3 p0 = itemsToCollect[i].transform.position;
                Vector3 p1 = finalTargetPosition;

                Vector3 c0 = p0 + Vector3.up * 2;
                Vector3 c1 = p1 + Vector3.up * 2;

                itemPaths[i].setPoints(new Vector3[] {p0, c1, c0, p1});
            }

            while(collectedItems < 3)
            {
                for (int i = 0; i < itemsToCollect.Count; i++)
                {
                    Item item = itemsToCollect[i];

                    if (item.transform.position == finalTargetPosition)
                        continue;

                    item.DisablePhysics();                    

                    float percent = Mathf.Clamp01((timer - (i * delayBetweenItems)) / animationDuration);

                    Vector3 targetPosition = itemPaths[i].point(percent);

                    item.transform.position = targetPosition;

                    // Multiplied percent by 1.1f to scale down faster
                    item.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, percent * 1.1f);


                    if (item.transform.position == finalTargetPosition)
                    {
                        ItemPickedUp?.Invoke(item);
                        collectedItems++;
                    }
                }

                // If the item has reached, increase the collected items count
                timer += Time.deltaTime;
                yield return null;
            }

            for (int i = itemsToCollect.Count - 1; i >= 0; i--)
                Destroy(itemsToCollect[i].gameObject);

            _isUsingPowerup = false;
        }

        [Button]
        public void VacuumPowerupPressed()
        {
            // Do we have enough powerups ? 
            if(_vacuumPUCount <= 0)
            {
                // Show a rewarded video and reward 3 powerups
                _vacuumPUCount += 3;
                SaveData();
            }
            else
            {
                // We have enough !
                _isUsingPowerup = true;
                
                _vacuumPUCount--;
                SaveData();

                // Play the vac animation
                _vacuumAnimator.Play("Activate");
            }
        }

        private ItemLevelData GetGreatestGoal(ItemLevelData[] goals)
        {
            int max = 0;
            int goalIndex = -1;

            for (int i = 0; i < goals.Length; i++)
            {
                if(goals[i].Amount >= max)
                {
                    max = goals[i].Amount;
                    goalIndex = i;
                }
            }

            return goals[goalIndex];

            // We could have written, only use if the array is small due to sorting
            // return goals.OrderByDescending(g => g.amount).FirstOrDefault();
        }

        [Button]
        public void SpringPowerup()
        {
            // In here, we want to grab an item on a random spot, and put it back in the play area
            ItemSpot spot = ItemSpotsManager.Instance.GetRandomOccupiedSpot();

            if (spot == null)
            {
                Debug.LogWarning("No full spot found");
                return;
            }

            Item itemToRelease = spot.Item;

            spot.Clear();

            itemToRelease.UnassignSpot();
            itemToRelease.EnablePhysics();
            itemToRelease.SetState(EItemState.Active);

            itemToRelease.transform.parent = LevelManager.Instance.ItemsParent;
            itemToRelease.transform.localPosition = Vector3.up * 3;
            itemToRelease.transform.localScale = Vector3.one;

            // If the item was a goal, we also need to increase the goal count
            ItemBackToGame?.Invoke(itemToRelease);
        }

        [Button]
        public void FanPowerup()
        {
            // Maybe grab all the items, and apply a random force
            Item[] items = LevelManager.Instance.Items;

            foreach (Item item in items)
                item.ApplyRandomForce(_fanMagnitude);
        }

        [Button]
        public void FreezeGunPowerup()
        {
            TimerManager.Instance.FreezeTimer();
        }

        private void UpdateVacuumVisuals()
        {
            _vacuumVideoIcon.SetActive(_vacuumPUCount <= 0);

            if (_vacuumPUCount <= 0)
                _vacuumAmountText.text = "";
            else
                _vacuumAmountText.text = _vacuumPUCount.ToString();
        }

        private void LoadData()
        {
            _vacuumPUCount = PlayerPrefs.GetInt("VacuumCount", _initialVacuumPUCount);
            UpdateVacuumVisuals();           
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt("VacuumCount", _vacuumPUCount);
        }
    }
}