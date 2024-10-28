using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
  public static PlayersManager Instance;
  public bool debug = false;

  [Header("Gameplay Customization Settings")]
  [SerializeField, Range(1, 8)] int amountOfPlayers = 2;
  [SerializeField] bool limitedFuel = true;
  [SerializeField] float fuelPerRound = 100f;

  [Header("References")]
  [SerializeField] GameObject playerPrefab;
  [SerializeField] Transform tankTurnIndicator;

  // Runtime variables
  // --------------------------------------------------

  private bool initialized = false;
  private List<GameObject> players = new List<GameObject>();
  private int currentPlayerIndex = 0;

  // Built-in methods
  // --------------------------------------------------

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    if (!Instance) Instance = this;
  }

  void Update()
  {
    if (!initialized) return;

    switch (LevelManager.GetState())
    {
      case GameState.Turn:
        HandleTurn();
        break;
      case GameState.TurnOutcome:
        // HandleTurnOutcome();
        break;
    }
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize()
  {
    if (initialized) return;

    if (debug) Debug.Log("Attaching listeners");
    LevelManager.OnPlayerTurnStart.AddListener(OnPlayerTurnStart);

    if (debug) Debug.Log("Creating players...");
    CreatePlayers();
    initialized = true;

    if (debug) Debug.Log("Players created!");
  }

  void CreatePlayers()
  {
    TerrainData terrainData = TerrainManager.Instance.GetTerrainData();

    if (!terrainData.Initialized)
      throw new System.Exception("PlayersManager: Terrain data must be initialized before creating players!");

    float[] playerPositions = new float[amountOfPlayers];
    float worldWidth = terrainData.TextureRenderObject.GetComponent<SpriteRenderer>().bounds.size.x;
    Transform textureRenderTransform = terrainData.TextureRenderObject.transform;
    float playerY = textureRenderTransform.position.y; // Fair enough for now

    for (int i = 0; i < amountOfPlayers; i++)
    {
      float terrainPartSize = worldWidth / (amountOfPlayers + 1);
      playerPositions[i] = terrainPartSize + terrainPartSize * i;
      float playerX = textureRenderTransform.position.x - worldWidth / 2 + playerPositions[i];
      GameObject player = Instantiate(playerPrefab, new Vector3(playerX, playerY, 0), Quaternion.identity);
      players.Add(player);
    }
  }

  void HandleTurn()
  {
    GameObject currentPlayer = players[currentPlayerIndex];
    // TODO: Refactor this to a static variable to prevent slow GetComponent calls
    TankPhysics tankPhysics = currentPlayer.GetComponent<TankPhysics>();

    float inputX = Input.GetAxis("Horizontal");
    tankPhysics.HandleMovement(inputX);
  }

  // Event handlers
  // --------------------------------------------------

  void OnPlayerTurnStart() { }
}
