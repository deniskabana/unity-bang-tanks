using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Loading,
    Ready,
    TurnPrepared,
    Turn,
    TurnOutcome,
    GameOver
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public bool debug = false;

    // Runtime variables
    // --------------------------------------------------

    private GameState GAME_STATE = GameState.Loading;

    // Events
    public static UnityEvent OnGameReady = new UnityEvent(); // Called when the game is ready to start
    public static UnityEvent OnGameStart = new UnityEvent(); // Called when the game starts
    public static UnityEvent OnPlayerTurnPrepared = new UnityEvent(); // Called when the player's turn is prepared to start
    public static UnityEvent OnPlayerTurnStart = new UnityEvent(); // Called when the player's turn to play starts
    public static UnityEvent OnPlayerTurnEnd = new UnityEvent(); // Called when the player's turn to play ends
    public static UnityEvent OnPlayerTurnConsequencesStart = new UnityEvent(); // Called when player's turn ends and their consequences start
    public static UnityEvent OnPlayerTurnConsequencesEnd = new UnityEvent(); // Called when player's consequences end
    public static UnityEvent OnGameOver = new UnityEvent(); // Called when the game is over

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
        Initialize();
        StartGame();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize()
    {
        GAME_STATE = GameState.Loading;
        if (debug) Debug.Log("GAME_STATE: Loading");

        TerrainManager.Instance.Initialize();
        PlayersManager.Instance.Initialize();

        GAME_STATE = GameState.Ready;
        if (debug) Debug.Log("GAME_STATE: Ready");
        OnGameReady.Invoke();
    }

    public static GameState GetState()
    {
        return Instance.GAME_STATE;
    }

    void SetState(GameState state)
    {
        GAME_STATE = state;
    }

    // State methods
    // --------------------------------------------------

    public void StartGame()
    {
        if (GAME_STATE != GameState.Ready) return;
        OnGameStart.Invoke();

        PreparePlayerTurn();
    }

    public void PreparePlayerTurn()
    {
        SetState(GameState.TurnPrepared);
        if (debug) Debug.Log("GAME_STATE: TurnPrepared");
        OnPlayerTurnPrepared.Invoke();

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        SetState(GameState.Turn);
        if (debug) Debug.Log("GAME_STATE: Turn");
        OnPlayerTurnStart.Invoke();
    }

    public void EndPlayerTurn()
    {
        if (GAME_STATE != GameState.Turn) return;

        SetState(GameState.TurnOutcome);
        if (debug) Debug.Log("GAME_STATE: TurnOutcome");
        OnPlayerTurnEnd.Invoke();
        OnPlayerTurnConsequencesStart.Invoke();
    }

    public void EndPlayerConsequences()
    {
        if (GAME_STATE != GameState.TurnOutcome) return;

        SetState(GameState.TurnPrepared);
        if (debug) Debug.Log("GAME_STATE: TurnPrepared");
        OnPlayerTurnConsequencesEnd.Invoke();

        PreparePlayerTurn();
    }

    public void EndGame()
    {
        SetState(GameState.GameOver);
        if (debug) Debug.Log("GAME_STATE: GameOver");
        OnGameOver.Invoke();
    }
}
