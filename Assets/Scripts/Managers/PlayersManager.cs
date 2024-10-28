using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayersManager : MonoBehaviour
{
  [SerializeField] bool debug = false;

  [Header("Level Settings")]
  [SerializeField, Range(1, 8)] int amountOfPlayers = 2;

  [Header("Gameplay Customization Settings")]
  [SerializeField] bool limitedFuel = true;
  [SerializeField] float fuelPerRound = 100f;

  [Header("References")]
  [SerializeField] GameObject playerPrefab;

  // Runtime variables
  // --------------------------------------------------

  public static PlayersManager Instance;
  private List<GameObject> players = new List<GameObject>();
  private int currentPlayerIndex = 0;
  public static UnityEvent OnPlayersInitialized = new UnityEvent();

  // Built-in methods
  // --------------------------------------------------

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    Instance = this;
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize()
  {
    CreatePlayers();
  }

  void CreatePlayers()
  {
    if (debug) Debug.Log("Creating players...");

    if (!TerrainManager.Instance.GetTerrainData().Initialized)
      throw new System.Exception("PlayersManager: Terrain data must be initialized before creating players!");

    float[] playerPositions = new float[amountOfPlayers];
    float textureWidth = TerrainManager.Instance.GetTerrainData().TextureWidth;

    for (int i = 0; i < amountOfPlayers; i++)
    {
      float terrainPartSize = textureWidth / (amountOfPlayers + 2);
      playerPositions[i] = terrainPartSize * (i + 1);
      GameObject player = Instantiate(playerPrefab, new Vector3(playerPositions[i], 0, 0), Quaternion.identity);
      players.Add(player);
    }

    // Perform player initialization
    foreach (GameObject player in players)
    {
      // player.GetComponent<Player>().Initialize(limitedFuel, maxFuel, enableRoundTimer, maxRoundDuration);
    }

    if (debug) Debug.Log("Players created!");
  }

  public int GetCurrentPlayerIndex()
  {
    return currentPlayerIndex;
  }

  public GameObject GetCurrentPlayerObject()
  {
    return players[currentPlayerIndex];
  }
}
