using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoxX.TheCollector
{
    public interface IGameStateListener
    {
        void GameStateChangedCallback(EGameState gameState);
    }
}