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
  [SerializeField] GameObject playerPrefab;

  // Runtime variables
  // --------------------------------------------------

  [System.Serializable]
  public struct PlayersState
  {
    public string name;
    public PlayerTank script;
  }
  private List<PlayersState> playersState = new List<PlayersState>();

  private bool initialized = false;
  private int currentPlayerIndex = 0;
  private int turnCounter = 0;
  private float outcomeTimer = 0;
  private GameplaySettings gs;
  private PlayerSkinSettings pss;
  PlayerSkin[] shuffledSkins;

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

    if (gs.enableActiveIndicator && LevelManager.GetState() == GameState.Turn)
    {
      activePlayerIndicator.transform.position = GetCurrentPlayer().transform.position + new Vector3(0, indicatorYOffset, 0);
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

    if (debug) Debug.Log("PlayersTurnManager: Initializing PlayerTurnIndicator");
    activePlayerIndicator.SetActive(gs.enableActiveIndicator);
    if (!gs.animateActiveIndicator)
    {
      GameObject indicatorChild = activePlayerIndicator.transform.GetChild(0).gameObject;
      indicatorChild.GetComponent<Animation>().enabled = false;
    }

    if (debug) Debug.Log("PlayersTurnManager: Attaching listeners");
    LevelManager.OnPlayerTurnStart.AddListener(OnPlayerTurnStart);
    LevelManager.OnPlayerTurnEnd.AddListener(OnPlayerTurnEnd);

    if (debug) Debug.Log("PlayersTurnManager: Creating players...");
    ShuffleSkins();
    ShufflePlayerNames();
    CreatePlayers();
    ShufflePlayers();
    initialized = true;

    if (debug) Debug.Log("PlayersTurnManager: Players created!");
  }

  void CreatePlayers()
  {
    float[] playerPositions = new float[gs.amountOfPlayers];
    Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();

    float worldWidth = terrainBounds.size.x;
    float playerY = terrainBounds.min.y + 10;

    for (int i = 0; i < gs.amountOfPlayers; i++)
    {

      if (gs.playerPositioning == GameplaySettings.PlayerPositioning.Random)
      {
        float playerMinDistance = worldWidth * 0.05f; // TODO: Make this a setting
        float sideSafeDistance = worldWidth * 0.1f; // TODO: Make this a setting
        bool positionValid;

        do
        {
          positionValid = true;
          playerPositions[i] = Random.Range(sideSafeDistance, worldWidth - sideSafeDistance);

          for (int j = 0; j < i; j++)
          {
            if (Mathf.Abs(playerPositions[i] - playerPositions[j]) < playerMinDistance)
            {
              positionValid = false;
              break;
            }
          }
        } while (!positionValid);
      }
      else
      {
        float terrainPartSize = worldWidth / (gs.amountOfPlayers + 1);
        playerPositions[i] = terrainPartSize + terrainPartSize * i;
      }

      float playerX = terrainBounds.min.x + playerPositions[i];
      GameObject playerObject = Instantiate(playerPrefab, new Vector3(playerX, playerY, 0), Quaternion.identity, playersParent);
      playerObject.transform.localScale = new Vector3(gs.playerScale, gs.playerScale, 1);
      PlayerTank playerTank = playerObject.GetComponent<PlayerTank>();
      playerTank.Initialize(i, shuffledSkins[i % shuffledSkins.Length]);
      playersState.Add(new PlayersState { name = PlayerNames.playerNames[i], script = playerTank });
    }
  }

  void OnPlayerTurnStart()
  {
    if (debug) Debug.Log("PlayersTurnManager: Player turn started for player " + currentPlayerIndex);
    if (gs.enableActiveIndicator) activePlayerIndicator.SetActive(true);

    UIManager.SetActiveTankHUDSkin(GetCurrentPlayer().GetSkin());
    UIManager.SetActiveTankHUDName(playersState[currentPlayerIndex].name);
    CameraManager.Zoom(true); // Zoom in
    CameraManager.TrackObject(GetCurrentPlayer().gameObject); // Track player

    turnCounter += 1;
    GetCurrentPlayer().HandleTurnStart();
  }

  void OnPlayerTurnEnd()
  {
    if (debug) Debug.Log("PlayersTurnManager: Player turn ended for player " + currentPlayerIndex);
    if (gs.enableActiveIndicator) activePlayerIndicator.SetActive(false);
    GetCurrentPlayer().HandleTurnEnd();

    currentPlayerIndex += 1;
    if (currentPlayerIndex >= playersState.Count) currentPlayerIndex = 0;
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

  PlayerTank GetCurrentPlayer()
  {
    return playersState[currentPlayerIndex].script;
  }

  void ShufflePlayers()
  {
    for (int i = playersState.Count - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      (playersState[j], playersState[i]) = (playersState[i], playersState[j]);
    }
  }

  void ShuffleSkins()
  {
    PlayerSkin[] newShuffledSkins = pss.playerSkins.ToArray();

    for (int i = newShuffledSkins.Length - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      (newShuffledSkins[j], newShuffledSkins[i]) = (newShuffledSkins[i], newShuffledSkins[j]);
    }

    shuffledSkins = newShuffledSkins;
  }

  // Player names
  // --------------------------------------------------

  void ShufflePlayerNames()
  {
    for (int i = PlayerNames.playerNames.Length - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      (PlayerNames.playerNames[j], PlayerNames.playerNames[i]) = (PlayerNames.playerNames[i], PlayerNames.playerNames[j]);
    }
  }
}
