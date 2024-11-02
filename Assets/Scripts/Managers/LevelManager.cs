using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Loading,
    Ready,
    Turn,
    TurnOutcome,
    GameOver
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public bool debug = false;

    [SerializeField] GameplaySettings gameplaySettings;
    [SerializeField] TerrainSettings terrainSettings;

    // Runtime variables
    // --------------------------------------------------

    private GameState GAME_STATE = GameState.Loading;

    // Events
    public static UnityEvent OnGameReady = new UnityEvent();
    public static UnityEvent OnGameStart = new UnityEvent();
    public static UnityEvent OnPlayerTurnStart = new UnityEvent();
    public static UnityEvent OnPlayerTurnEnd = new UnityEvent();
    public static UnityEvent OnPlayerTurnOutcomeStart = new UnityEvent();
    public static UnityEvent OnPlayerTurnOutcomeEnd = new UnityEvent();
    public static UnityEvent OnGameOver = new UnityEvent();

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
        SetState(GameState.Loading);

        TerrainManager.Instance.Initialize(terrainSettings);
        PlayersTurnManager.Instance.Initialize(gameplaySettings);

        SetState(GameState.Ready);
        OnGameReady.Invoke();
    }

    public static GameState GetState()
    {
        return Instance.GAME_STATE;
    }

    void SetState(GameState state)
    {
        GAME_STATE = state;
        if (debug) Debug.Log("GAME_STATE: " + state);
    }

    // State methods
    // --------------------------------------------------

    public void StartGame()
    {
        if (GAME_STATE != GameState.Ready) return;
        OnGameStart.Invoke();

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        SetState(GameState.Turn);
        OnPlayerTurnStart.Invoke();
    }

    public void EndPlayerTurn() // Also acts as StartPlayerOutcome
    {
        if (GAME_STATE != GameState.Turn) return;
        OnPlayerTurnEnd.Invoke();
        // Start player outcome
        OnPlayerTurnOutcomeStart.Invoke();
        SetState(GameState.TurnOutcome);
    }

    public void EndPlayerOutcome()
    {
        if (GAME_STATE != GameState.TurnOutcome) return;
        OnPlayerTurnOutcomeEnd.Invoke();
        // Start next turn
        StartPlayerTurn();
    }

    public void EndGame()
    {
        SetState(GameState.GameOver);
        OnGameOver.Invoke();
    }
}
