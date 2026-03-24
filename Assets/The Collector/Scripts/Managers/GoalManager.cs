using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoxX.TheCollector
{
    public class GoalManager : MonoBehaviour
    {
        public static GoalManager Instance;

        [Header(" Elements ")]
        [SerializeField] private Transform _cardsParent;
        [SerializeField] private GoalCard _goalCardPrefab;

        [Header(" Data ")]
        private ItemLevelData[] _goals;
        public ItemLevelData[] Goals => _goals;

        private List<GoalCard> _goalCards;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            LevelManager.levelSpawned       += OnLevelSpawned;
            ItemSpotsManager.ItemPickedUp   += OnItemPickedUp;
            
            PowerupManager.ItemPickedUp     += OnItemPickedUp;
            PowerupManager.ItemBackToGame   += OnItemBackToGame;

        }

        private void OnDestroy()
        {
            LevelManager.levelSpawned       -= OnLevelSpawned;    
            ItemSpotsManager.ItemPickedUp   -= OnItemPickedUp;

            PowerupManager.ItemPickedUp     -= OnItemPickedUp;
            PowerupManager.ItemBackToGame   -= OnItemBackToGame;
        }


        private void OnLevelSpawned(Level level)
        {
            // Configure the goals
            GenerateGoalCards(level.GetGoals());
        }

        private void GenerateGoalCards(ItemLevelData[] goals)
        {
            this._goals = goals;

            for (int i = 0; i < goals.Length; i++)
                GenerateGoalCard(goals[i]);            
        }

        private void GenerateGoalCard(ItemLevelData goal)
        {
            GoalCard cardInstance = Instantiate(_goalCardPrefab, _cardsParent);

            cardInstance.Configure(goal.ItemPrefab.Icon, goal.Amount);

            _goalCards.Add(cardInstance);
        }

        private void OnItemPickedUp(Item item)
        {
            // When the item is picked up, reduce the amount of our goals, and update the UI
            // We could use a dictionnary with Key = itemName & Value = GoalCard

            for (int i = 0; i < _goals.Length; i++)
            {
                if (_goals[i].ItemPrefab.name != item.name)
                    continue;

                _goals[i].Amount--;

                // Complete the goal
                if (_goals[i].Amount <= 0)
                    CompleteGoal(i);
                else
                    _goalCards[i].UpdateAmount(_goals[i].Amount);

                break;
            }
        }
        
        private void CompleteGoal(int goalIndex)
        {
            _goalCards[goalIndex].Complete();

            CheckForLevelComplete();
        }

        private void CheckForLevelComplete()
        {
            foreach (GoalCard card in _goalCards)
                if (!card.IsComplete)
                    return;

            GameManager.Instance.SetGameState(EGameState.LEVELCOMPLETE);
        }

        private void OnItemBackToGame(Item item)
        {
            for (int i = 0; i < _goals.Length; i++)
            {
                if (_goals[i].ItemPrefab.name != item.name)
                    continue;

                _goals[i].Amount++;

                _goalCards[i].UpdateAmount(_goals[i].Amount);

                break;
            }
        }
    }
}