using UnityEngine;
using System;

namespace FoxX.TheCollector
{
    public class LevelManager : MonoBehaviour, IGameStateListener
    {
        public static LevelManager Instance;

        [Header(" Data ")]
        [SerializeField] private Level[] levels;
        public const string levelKey = "Level";

        [Header(" Settings ")]
        private Level _currentLevel;
        private int _levelIndex;

        [Header(" Elements ")]
        public Item[] Items => _currentLevel.GetItems();
        public Transform ItemsParent => _currentLevel.ItemsParent;

        [Header(" Actions ")]
        public static Action<Level> levelSpawned;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            LoadData();
        }


        public void GameStateChangedCallback(EGameState gameState)
        {
            if (gameState == EGameState.GAME)
                SpawnLevel();
            else if (gameState == EGameState.LEVELCOMPLETE)
                HandleLevelComplete();
        }

        private void SpawnLevel()
        {
            // Remove any previous level
            transform.Clear();

            // Validate the level Index to prevent out of range
            int validatedLevelIndex = _levelIndex % levels.Length;

            // Spawn the level
            _currentLevel = Instantiate(levels[validatedLevelIndex], transform);

            // Trigger an action
            levelSpawned?.Invoke(_currentLevel);
        }

        private void HandleLevelComplete()
        {
            _levelIndex++;
            SaveData();
        }

        private void LoadData()
        {
            _levelIndex = PlayerPrefs.GetInt(levelKey);
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt(levelKey, _levelIndex);
        }

    }
}