using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] bool debug = false;

    // Runtime variables
    // --------------------------------------------------

    public static LevelManager Instance;

    private GameState gameState = GameState.Loading;
    private TerrainManager terrainManager;
    private PlayersManager playersManager;

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Instance = this;
        Initialize();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize()
    {
        gameState = GameState.Loading;
        if (debug) Debug.Log("Game state: Loading");

        terrainManager = TerrainManager.Instance;
        playersManager = PlayersManager.Instance;

        // TODO: Implement game initialization logic

        gameState = GameState.PlayerTurn;
        if (debug) Debug.Log("Game state: PlayerTurn");
    }

}
