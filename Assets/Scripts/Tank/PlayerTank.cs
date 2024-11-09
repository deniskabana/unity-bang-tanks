using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct PlayerState
{
  public bool isPlayingTurn;
  public int health;
  public float fuel;
  public enum TurnStage { Moving, Aiming, Shooting }
  public TurnStage turnStage;

  // Shooting, weapon choice, etc.
  // TODO: prepare for multiple weapons
}

[@RequireComponent(typeof(TankPhysics))]
[@RequireComponent(typeof(TankShooting))]
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

  private int selfPlayerIndex;

  // Built-in methods
  // --------------------------------------------------

  void Start()
  {
    physicsScript = GetComponent<TankPhysics>();
    shootingScript = GetComponent<TankShooting>();
  }

  void Update()
  {
    if (!state.isPlayingTurn) return;

    float inputX = 0;

    if (ControlsManager.GetInput(ControlType.Left)) inputX = -1;
    if (ControlsManager.GetInput(ControlType.Right)) inputX = 1;

    if (ControlsManager.GetInput(ControlType.Shoot))
    {
    }

    if (state.turnStage == PlayerState.TurnStage.Moving)
    {
      // Handling horizontal movement
      if (inputX != 0 && state.fuel > 0)
      {
        if (physicsScript.HandleMovement(inputX) != 0)
        {
          state.fuel -= fuelDepletionRate * Time.deltaTime;

          GameplaySettings gs = LevelManager.Instance.gameplaySettings;
          UIManager.UpdateActiveGasBar(state.fuel / gs.maxFuelPerRound);
        }
      }
    }

    if (state.turnStage == PlayerState.TurnStage.Aiming)
    {
      // Handling cannon rotation
      if (inputX != 0) shootingScript.HandleAiming(inputX * -1);
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

    ControlsManager.OnControlDown.AddListener(OnShootButtonPressed);
    ControlsManager.OnControlUp.AddListener(OnShootButtonReleased);
  }

  void SetInitialState()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = gs.maxPlayerHealth,
      fuel = gs.maxFuelPerRound,
      turnStage = PlayerState.TurnStage.Moving
    };
  }

  public void HandleTurnStart()
  {
    state.isPlayingTurn = true;
    state.fuel = LevelManager.Instance.gameplaySettings.maxFuelPerRound;
    state.turnStage = PlayerState.TurnStage.Moving;
    playerIndicator.gameObject.SetActive(true);
    UIManager.ShowTouchAimButton();
    UIManager.UpdateActiveGasBar(1);
    UIManager.UpdateActiveArmorBar(state.health / LevelManager.Instance.gameplaySettings.maxPlayerHealth);
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
    playerIndicator.gameObject.SetActive(false);
    UIManager.ShowTouchAimButton();
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

  public void TakeDamage(int damage)
  {
    state.health -= damage;
    UIManager.UpdateActiveArmorBar(state.health / LevelManager.Instance.gameplaySettings.maxPlayerHealth);

    if (state.health <= 0) LevelManager.Instance.GameOver();
  }

  // Event handlers
  // --------------------------------------------------

  void OnShootButtonReleased(ControlType controlType)
  {
    if (controlType != ControlType.Shoot || !state.isPlayingTurn) return;

    switch (state.turnStage)
    {
      case PlayerState.TurnStage.Moving:
        state.turnStage = PlayerState.TurnStage.Aiming;
        UIManager.ShowTouchShootButton();
        break;
      case PlayerState.TurnStage.Shooting:
        shootingScript.Shoot(UnityEngine.Random.Range(300, 1000));
        LevelManager.Instance.EndPlayerTurn();
        break;
    }
  }

  void OnShootButtonPressed(ControlType controlType)
  {
    if (controlType != ControlType.Shoot || !state.isPlayingTurn) return;

    switch (state.turnStage)
    {
      case PlayerState.TurnStage.Aiming:
        state.turnStage = PlayerState.TurnStage.Shooting;
        break;
    }
  }
}