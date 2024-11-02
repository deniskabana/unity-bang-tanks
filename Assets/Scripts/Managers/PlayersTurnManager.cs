using System.Collections.Generic;
using UnityEngine;

public class PlayersTurnManager : MonoBehaviour
{
  public static PlayersTurnManager Instance;
  public bool debug = false;

  [Header("References")]
  [SerializeField] GameObject playerPrefab;

  // Runtime variables
  // --------------------------------------------------

  private bool initialized = false;
  private readonly List<PlayerTank> players = new List<PlayerTank>();
  private int currentPlayerIndex = 0;
  private GameplaySettings gs;

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
      GameObject playerObject = Instantiate(playerPrefab, new Vector3(playerX, playerY, 0), Quaternion.identity);
      PlayerTank playerTank = playerObject.GetComponent<PlayerTank>();
      players.Add(playerTank);
    }
  }

  // Event handlers
  // --------------------------------------------------

  void OnPlayerTurnStart()
  {
    if (debug) Debug.Log("Player turn started!");
  }

  void OnPlayerTurnEnd()
  {
    if (debug) Debug.Log("Player turn ended!");
    currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
  }
}
