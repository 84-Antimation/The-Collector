using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FoxX.TheCollector
{
    public class TimerManager : MonoBehaviour, IGameStateListener
    {
        public static TimerManager Instance;

        [Header(" Elements ")]
        [SerializeField] private TextMeshProUGUI _timerText;
        private int _timerSeconds;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            LevelManager.levelSpawned += OnLevelSpawned;
        }

        private void OnDestroy()
        {
            LevelManager.levelSpawned -= OnLevelSpawned;
        }

        public void GameStateChangedCallback(EGameState gameState)
        {
            switch (gameState)
            {
                case EGameState.GAME:
                    StartTimer();
                    break;

                case EGameState.LEVELCOMPLETE:
                    StopTimer();
                    StopAllCoroutines();
                    break;
            }
        }

        private void OnLevelSpawned(Level level)
        {
            _timerSeconds = level.Duration;
            _timerText.text = SecondsToString(_timerSeconds);
        }

        public void StartTimer()
        {
            InvokeRepeating("UpdateTimer", 1, 1);
        }

        private void UpdateTimer()
        {
            _timerSeconds--;
            _timerText.text = SecondsToString(_timerSeconds);

            if (_timerSeconds <= 0)
                TimerFinished();
        }

        private void TimerFinished()
        {
            StopTimer();

            GameManager.Instance.SetGameState(EGameState.GAMEOVER);
        }

        public void StopTimer()
        {
            CancelInvoke("UpdateTimer");
        }

        public void FreezeTimer()
        {
            StopTimer();
            StartCoroutine("FreezeTimerCoroutine");
        }

        IEnumerator FreezeTimerCoroutine()
        {
            yield return new WaitForSeconds(10);
            StartTimer();
        }

        public int GetTimerSeconds()
        {
            return _timerSeconds;
        }

        private string SecondsToString(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString().Substring(3);
        }
    }
}