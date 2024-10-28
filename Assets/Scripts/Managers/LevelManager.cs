using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Loading,
    PlayerTurn,
    PlayerTurnConsequences,
    GameOver
}

public class LevelManager : MonoBehaviour
{
    // Private variables
    // --------------------------------------------------

    private GameState gameState = GameState.Loading;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        Initialize();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize()
    {
        gameState = GameState.PlayerTurn;
    }
}
