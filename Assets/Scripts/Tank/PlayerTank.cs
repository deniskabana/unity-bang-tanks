using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct PlayerState
{
  public bool isPlayingTurn;
  public int health;
  public int maxHealth;
  public float energy;
  public enum TurnStage { Moving, Aiming, Shooting }
  public TurnStage turnStage;

  // Upgrades
  public int batteryCapacity;
  public int batteryRechargeRate;
  public int batteryAmount;

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
  private PlayerSkin skin;
  private float lastShotStrengthValue = -1;

  private int selfPlayerIndex;

  // Built-in methods
  // --------------------------------------------------

  void FixedUpdate()
  {
    if (!state.isPlayingTurn) return;

    float inputX = 0;

    if (ControlsManager.GetInput(ControlType.Left)) inputX = -1;
    if (ControlsManager.GetInput(ControlType.Right)) inputX = 1;

    if (state.turnStage == PlayerState.TurnStage.Moving)
    {
      // Handling horizontal movement
      if (inputX != 0 && state.energy > 0 && physicsScript.isGrounded)
      {
        // Deplete fuel even if the tank can not move
        state.energy -= LevelManager.Instance.gameplaySettings.energyDepletionRate * Time.deltaTime;
        GameplaySettings gs = LevelManager.Instance.gameplaySettings;
        UIManager.UpdateActiveBatteryBars(state.energy, state.batteryCapacity, state.batteryAmount);
        physicsScript.HandleMovement(inputX);
      }
    }

    if (state.turnStage == PlayerState.TurnStage.Aiming)
    {
      // Handling cannon rotation
      if (inputX != 0) shootingScript.HandleAiming(inputX * -1);
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.CompareTag("Projectiles"))
    {
      TakeDamage(25);
      Debug.Log("Player hit by projectile");
      return;
    }
    if (collision.gameObject.CompareTag("Explosion"))
    {
      TakeDamage(10);
      Debug.Log("Player hit by explosion");
      return;
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

    ControlsManager.OnControlUp.AddListener(OnShootButtonReleased);
  }

  void SetInitialState()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = gs.maxPlayerHealth / 2,
      maxHealth = gs.maxPlayerHealth,
      energy = gs.batteryCapacity * gs.batteryAmountStart,
      turnStage = PlayerState.TurnStage.Moving,
      batteryCapacity = gs.batteryCapacity,
      batteryAmount = gs.batteryAmountStart,
      batteryRechargeRate = gs.batteryRechargeRate
    };
  }

  public void HandleTurnStart()
  {
    state.isPlayingTurn = true;
    state.energy = Mathf.Min(state.energy + state.batteryRechargeRate * state.batteryCapacity, state.batteryCapacity * state.batteryAmount);
    state.turnStage = PlayerState.TurnStage.Moving;
    UIManager.UpdateActiveBatteryBars(state.energy, state.batteryCapacity, state.batteryAmount);
    UIManager.UpdateActiveHealthBar(state.health, state.maxHealth);
    UIManager.SetTouchControlsStage(state.turnStage);
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
  }

  public void TakeDamage(int damage)
  {
    Debug.Log("Player took damage: " + damage);
    state.health -= damage;
    UIManager.UpdateActiveHealthBar(state.health, state.maxHealth);

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
        UIManager.SetTouchControlsStage(state.turnStage);
        break;

      case PlayerState.TurnStage.Aiming:
        state.turnStage = PlayerState.TurnStage.Shooting;
        UIManager.SetTouchControlsStage(state.turnStage);
        UIManager.AnimateStrengthIndicator();
        if (lastShotStrengthValue != -1) UIManager.SetStrengthIndicatorGhostValue(lastShotStrengthValue);
        break;

      case PlayerState.TurnStage.Shooting:
        float strengthValue = UIManager.GetStrengthIndicatorValue();
        float shotStrength = Mathf.Lerp(LevelManager.Instance.gameplaySettings.minShotStrength, LevelManager.Instance.gameplaySettings.maxShotStrength, strengthValue);
        GameObject bullet = shootingScript.Shoot(shotStrength);

        // Resets after end of player turn
        CameraManager.TrackObject(bullet); // Track bullet
        UIManager.SetTouchControlsStage(null); // Reset touch controls
        UIManager.StopStrengthIndicator(); // Reset strength indicator
        lastShotStrengthValue = strengthValue; // Remember last shot strength

        // End turn
        // TODO: Set the game to waiting mode until projectiles are destroyed or maxTime is reached
        // TODO: After waiting made, end player turn and start consequences timer
        LevelManager.Instance.EndPlayerTurn();
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