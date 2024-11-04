using System.Collections.Generic;
using UnityEngine;

public class PlayersTurnManager : MonoBehaviour
{
  public static PlayersTurnManager Instance;
  public bool debug = false;

  [Header("Settings")]
  [SerializeField] int minOutcomeDurationSeconds = 3;

  [Header("References")]
  [SerializeField] Transform playersParent;
  [SerializeField] GameObject playerPrefab;
  [SerializeField] Transform mainCamera;

  // Runtime variables
  // --------------------------------------------------

  private bool initialized = false;
  private readonly List<PlayerTank> players = new List<PlayerTank>();
  private int currentPlayerIndex = 0;
  private int turnCounter = 0;
  private float outcomeTimer = 0;
  private GameplaySettings gs;

  private float cameraSizeInTurn = 4f;
  private float cameraSizeInOutcome = 8f;
  private Camera mainCameraComponent;

  // Built-in methods
  // --------------------------------------------------

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    if (!Instance) Instance = this;
    mainCameraComponent = mainCamera.GetComponent<Camera>();
  }

  void Update()
  {
    if (!initialized) return;

    // Camera movement and management
    switch (LevelManager.GetState())
    {
      case GameState.Turn:
        mainCameraComponent.orthographicSize = Mathf.Lerp(mainCameraComponent.orthographicSize, cameraSizeInTurn, Time.deltaTime * 1.75f);
        Vector3 targetPosition = players[currentPlayerIndex].transform.position + new Vector3(0, 0, -10);
        mainCamera.position = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * 5f);
        break;
      case GameState.TurnOutcome:
        mainCameraComponent.orthographicSize = Mathf.Lerp(mainCameraComponent.orthographicSize, cameraSizeInOutcome, Time.deltaTime * 1.75f);
        break;
    }

    if (outcomeTimer > 0)
    {
      outcomeTimer -= Time.deltaTime;
      if (outcomeTimer <= 0)
      {
        ExecuteOutcome();
      }
    }
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize(GameplaySettings _gameplaySettings)
  {
    if (initialized) return;
    gs = _gameplaySettings;

    if (debug) Debug.Log("Attaching listeners");
    LevelManager.OnPlayerTurnStart.AddListener(OnPlayerTurnStart);
    LevelManager.OnPlayerTurnEnd.AddListener(OnPlayerTurnEnd);

    if (debug) Debug.Log("Creating players...");
    CreatePlayers();
    initialized = true;

    if (debug) Debug.Log("Players created!");
  }

  void CreatePlayers()
  {
    float[] playerPositions = new float[gs.amountOfPlayers];
    Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();

    float worldWidth = terrainBounds.size.x;
    float playerY = terrainBounds.min.y + 1;

    for (int i = 0; i < gs.amountOfPlayers; i++)
    {
      float terrainPartSize = worldWidth / (gs.amountOfPlayers + 1);
      playerPositions[i] = terrainPartSize + terrainPartSize * i;

      float playerX = terrainBounds.min.x + playerPositions[i];
      GameObject playerObject = Instantiate(playerPrefab, new Vector3(playerX, playerY, 0), Quaternion.identity, playersParent);
      PlayerTank playerTank = playerObject.GetComponent<PlayerTank>();
      playerTank.Initialize();
      players.Add(playerTank);
    }
  }

  // Event handlers
  // --------------------------------------------------

  void OnPlayerTurnStart()
  {
    if (debug) Debug.Log("Player turn started for player " + currentPlayerIndex);
    turnCounter += 1;
    players[currentPlayerIndex].HandleTurnStart();
  }

  void OnPlayerTurnEnd()
  {
    if (debug) Debug.Log("Player turn ended for player " + currentPlayerIndex);
    players[currentPlayerIndex].HandleTurnEnd();

    currentPlayerIndex += 1;
    if (currentPlayerIndex >= players.Count) currentPlayerIndex = 0;
    StartOutcomeTimer();
  }

  void StartOutcomeTimer()
  {
    outcomeTimer = minOutcomeDurationSeconds;
  }
  void ExecuteOutcome()
  {
    LevelManager.Instance.EndPlayerOutcome();
  }
}
