using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

namespace FoxX.TheCollector
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        private EGameState _gameState;

        [Header(" Actions ")]
        public static Action OnGamePaused;
        public static Action OnGameResumed;

        [Header(" Settings ")]
        public bool IsGame => _gameState == EGameState.GAME;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 60;

            SetGameState(EGameState.MENU);
        }

        public void SetGameState(EGameState gameState)
        {
            this._gameState = gameState;

            // We can cache these, if they don't change over time
            IEnumerable<IGameStateListener> gameStateDependencies =
                FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IGameStateListener>();

            foreach (IGameStateListener dependency in gameStateDependencies)
                dependency.GameStateChangedCallback(gameState);
        }

        public void StartGame()
        {
            SetGameState(EGameState.GAME);
        }
        
        public void NextButtonCallback()
        {
            SceneManager.LoadScene(0);
        }

        public void RetryButtonCallback()
        {
            SceneManager.LoadScene(0);
        }
    }
}