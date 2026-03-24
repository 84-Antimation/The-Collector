using UnityEngine;
using TMPro;

namespace FoxX.TheCollector
{
    public class UIManager : MonoBehaviour, IGameStateListener
    {
        [Header(" Panels ")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _levelComplete;
        [SerializeField] private GameObject _gameover;

        [Header(" Elements ")]
        [SerializeField] private TextMeshProUGUI _levelText;

        private void Start()
        {
            _levelText.text = "Level " + (PlayerPrefs.GetInt(LevelManager.levelKey) + 1);
        }

        public void GameStateChangedCallback(EGameState gameState)
        {
            _menuPanel.SetActive(gameState == EGameState.MENU);
            _gamePanel.SetActive(gameState == EGameState.GAME);
            _levelComplete.SetActive(gameState == EGameState.LEVELCOMPLETE);
            _gameover.SetActive(gameState == EGameState.GAMEOVER);
        }
    }
}