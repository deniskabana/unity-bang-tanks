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
[@RequireComponent(typeof(TankHealth))]
[@RequireComponent(typeof(TankShooting))]
public class PlayerTank : MonoBehaviour
{
  [SerializeField] Transform body1;
  [SerializeField] Transform body2;

  // Runtime variables
  // --------------------------------------------------

  private PlayerState state;
  private TankPhysics physicsScript;
  private TankShooting shootingScript;
  PlayerSkin skin;

  private int selfPlayerIndex;

  readonly float fuelDepletionRate = 20f;

  // Built-in methods
  // --------------------------------------------------

  void FixedUpdate()
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
      if (inputX != 0 && state.fuel > 0 && physicsScript.isGrounded)
      {
        // Deplete fuel even if the tank can not move
        state.fuel -= fuelDepletionRate * Time.deltaTime;
        GameplaySettings gs = LevelManager.Instance.gameplaySettings;
        UIManager.UpdateActiveGasBar(state.fuel / gs.maxFuelPerRound);
        physicsScript.HandleMovement(inputX);
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

  public void Initialize(int index, PlayerSkin skin)
  {
    physicsScript = gameObject.GetComponent<TankPhysics>();
    shootingScript = gameObject.GetComponent<TankShooting>();

    selfPlayerIndex = index;
    SetInitialState();
    SetSkin(skin);

    shootingScript.Initialize();
    physicsScript.Initialize();

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
    UIManager.ShowTouchAimButton();
    UIManager.UpdateActiveGasBar(1);
    UIManager.UpdateActiveArmorBar(state.health / LevelManager.Instance.gameplaySettings.maxPlayerHealth);
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
    UIManager.ShowTouchAimButton();
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
        float strengthValue = UIManager.GetStrengthIndicatorValue();
        float shotStrength = Mathf.Lerp(LevelManager.Instance.gameplaySettings.minShotStrength, LevelManager.Instance.gameplaySettings.maxShotStrength, strengthValue);
        GameObject bullet = shootingScript.Shoot(shotStrength);
        CameraManager.Zoom(false); // Zoom out
        CameraManager.TrackObject(bullet); // Track bullet
        UIManager.HideStrengthIndicator();
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
        UIManager.ShowStrengthIndicator();
        break;
    }
  }

  public void SetSkin(PlayerSkin _skin)
  {
    skin = _skin;
    body1.GetComponent<SpriteRenderer>().sprite = skin.body1;
    body2.GetComponent<SpriteRenderer>().sprite = skin.body2;
  }

  public PlayerSkin GetSkin()
  {
    return skin;
  }
}