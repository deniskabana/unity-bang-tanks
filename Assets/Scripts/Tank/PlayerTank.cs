using System;
using UnityEngine;

[Serializable]
public struct PlayerState
{
  public bool isPlayingTurn;
  public int health;
  public float fuel;

  // Shooting, weapon choice, etc.
  // TODO: prepare for multiple weapons
}

[@RequireComponent(typeof(TankPhysics))]
[@RequireComponent(typeof(TankShooting))]
[@RequireComponent(typeof(TankHealth))]
public class PlayerTank : MonoBehaviour
{
  [SerializeField] float fuelDepletionRate = 20f;

  [Header("References")]
  [SerializeField] Transform playerIndicator;
  [SerializeField] Transform tankBody;
  [SerializeField] Transform tankCannon;

  // Runtime variables
  // --------------------------------------------------

  private PlayerState state;
  private TankPhysics physicsScript;
  private TankShooting shootingScript;
  private TankHealth healthScript;

  private int selfPlayerIndex;

  // Built-in methods
  // --------------------------------------------------

  void Start()
  {
    physicsScript = GetComponent<TankPhysics>();
    shootingScript = GetComponent<TankShooting>();
    healthScript = GetComponent<TankHealth>();
  }

  void Update()
  {
    if (!state.isPlayingTurn) return;

    float inputX = Input.GetAxis("Horizontal"); // Horizontal movement
    float inputY = Input.GetAxis("Vertical"); // Cannon rotation

    // Handling horizontal movement
    if (inputX != 0 && state.fuel > 0)
    {
      if (physicsScript.HandleMovement(inputX) != 0)
      {
        state.fuel -= fuelDepletionRate * Time.deltaTime;
      }
    }

    // Handling cannon rotation
    if (inputY != 0)
    {
      shootingScript.AimCannon(inputY);
    }

    bool isHoldingFire = Input.GetKeyUp(KeyCode.Space);
    if (isHoldingFire)
    {
      shootingScript.Shoot(UnityEngine.Random.Range(300, 1000));
      LevelManager.Instance.EndPlayerTurn();
    }
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize(int index)
  {
    selfPlayerIndex = index;
    SetInitialState();
    SetPlayerColor();
    playerIndicator.gameObject.SetActive(false);
  }

  void SetInitialState()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = gs.maxPlayerHealth,
      fuel = gs.maxFuelPerRound,
    };
  }

  public void HandleTurnStart()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state.isPlayingTurn = true;
    state.fuel = gs.maxFuelPerRound;
    playerIndicator.gameObject.SetActive(true);
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
    playerIndicator.gameObject.SetActive(false);
  }

  public void SetPlayerColor()
  {
    PlayerColorSprites[] playerColors = PlayersTurnManager.Instance.playerColors;
    if (playerColors.Length < 2) return;
    int colorIndex = selfPlayerIndex % playerColors.Length;
    PlayerColorSprites colorSprites = playerColors[colorIndex];
    tankBody.GetComponent<SpriteRenderer>().sprite = colorSprites.body;
    tankCannon.GetComponent<SpriteRenderer>().sprite = colorSprites.cannon;
  }
}