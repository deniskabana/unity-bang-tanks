using System.Collections.Generic;
using UnityEngine;

public class PlayersTurnManager : MonoBehaviour
{
  public static PlayersTurnManager Instance;
  public bool debug = false;

  [Header("Settings")]
  [SerializeField] int minOutcomeDurationSeconds = 3;
  [SerializeField] float indicatorYOffset = 2.25f;

  [Header("References")]
  [SerializeField] GameObject activePlayerIndicator;
  [SerializeField] Transform playersParent;
  [SerializeField] Transform mainCamera;
  [SerializeField] GameObject playerPrefab;

  // Runtime variables
  // --------------------------------------------------

  private bool initialized = false;
  private readonly List<PlayerTank> players = new List<PlayerTank>();
  private int currentPlayerIndex = 0;
  private int turnCounter = 0;
  private float outcomeTimer = 0;
  private GameplaySettings gs;
  private PlayerSkinSettings pss;
  PlayerSkin[] shuffledSkins;

  private float cameraSizeInTurn = 5f;
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
        activePlayerIndicator.transform.position = players[currentPlayerIndex].transform.position + new Vector3(0, indicatorYOffset, 0);
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
        FinishOutcome();
      }
    }
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize(GameplaySettings _gameplaySettings, PlayerSkinSettings _playerSkinSettings)
  {
    if (initialized) return;
    gs = _gameplaySettings;
    pss = _playerSkinSettings;

    if (debug) Debug.Log("Attaching listeners");
    LevelManager.OnPlayerTurnStart.AddListener(OnPlayerTurnStart);
    LevelManager.OnPlayerTurnEnd.AddListener(OnPlayerTurnEnd);

    if (debug) Debug.Log("Creating players...");
    ShuffleSkins();
    CreatePlayers();
    ShufflePlayers();
    initialized = true;

    if (debug) Debug.Log("Players created!");
  }

  void CreatePlayers()
  {
    float[] playerPositions = new float[gs.amountOfPlayers];
    Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();

    float worldWidth = terrainBounds.size.x;
    float playerY = terrainBounds.min.y + 10;

    for (int i = 0; i < gs.amountOfPlayers; i++)
    {
      float terrainPartSize = worldWidth / (gs.amountOfPlayers + 1);
      playerPositions[i] = terrainPartSize + terrainPartSize * i;

      float playerX = terrainBounds.min.x + playerPositions[i];
      GameObject playerObject = Instantiate(playerPrefab, new Vector3(playerX, playerY, 0), Quaternion.identity, playersParent);
      playerObject.transform.localScale = new Vector3(gs.playerScale, gs.playerScale, 1);
      PlayerTank playerTank = playerObject.GetComponent<PlayerTank>();
      playerTank.Initialize(i, shuffledSkins[i % shuffledSkins.Length]);
      players.Add(playerTank);
    }
  }

  void OnPlayerTurnStart()
  {
    if (debug) Debug.Log("Player turn started for player " + currentPlayerIndex);
    activePlayerIndicator.SetActive(true);

    UIManager.SetActiveTankHUDImage(players[currentPlayerIndex].GetSkin().preview);
    UIManager.SetActiveTankHUDName("Play " + (currentPlayerIndex + 1));

    turnCounter += 1;
    players[currentPlayerIndex].HandleTurnStart();
  }

  void OnPlayerTurnEnd()
  {
    if (debug) Debug.Log("Player turn ended for player " + currentPlayerIndex);
    activePlayerIndicator.SetActive(false);
    players[currentPlayerIndex].HandleTurnEnd();

    currentPlayerIndex += 1;
    if (currentPlayerIndex >= players.Count) currentPlayerIndex = 0;
    StartOutcomeTimer();
  }

  void StartOutcomeTimer()
  {
    outcomeTimer = minOutcomeDurationSeconds;
  }

  void FinishOutcome()
  {
    LevelManager.Instance.EndPlayerOutcome();
  }

  void ShufflePlayers()
  {
    for (int i = players.Count - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      PlayerTank temp = players[i];
      players[i] = players[j];
      players[j] = temp;
    }
  }

  void ShuffleSkins()
  {
    PlayerSkin[] newShuffledSkins = pss.playerSkins.ToArray();

    for (int i = newShuffledSkins.Length - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      PlayerSkin temp = newShuffledSkins[i];
      newShuffledSkins[i] = newShuffledSkins[j];
      newShuffledSkins[j] = temp;
    }

    shuffledSkins = newShuffledSkins;
  }
}
