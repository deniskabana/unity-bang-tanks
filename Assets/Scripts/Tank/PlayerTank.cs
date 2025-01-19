using System;
using TMPro;
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

  [Header("Smoke Effects")]
  [SerializeField] GameObject smokeEffectSM;
  [SerializeField] GameObject smokeEffectMD;
  [SerializeField] GameObject smokeEffectLG;

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
        UIManager.UpdateActiveBatteryBars(state.energy, state.batteryCapacity, state.batteryAmount, state.batteryRechargeRate);
        float moveSpeed = physicsScript.HandleMovement(inputX);
        if (moveSpeed != 0) state.energy -= LevelManager.Instance.gameplaySettings.energyDepletionRate * Time.deltaTime;
      }
    }

    if (state.turnStage == PlayerState.TurnStage.Aiming)
    {
      // Handling cannon rotation
      if (inputX != 0) shootingScript.HandleAiming(inputX * -1);
    }
  }

  void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.gameObject.CompareTag("Explosion"))
    {
      float minDamageRatio = 0.5f;
      float radius = collision.GetComponent<CircleCollider2D>().radius;
      float maxDamage = collision.GetComponent<ExplosionDamage>().explosionDamage; // Bullet will pass the damage value to explosion
      float distance = Vector2.Distance(transform.position, collision.transform.position);
      float tolerance = radius * 0.05f;
      int damage = Mathf.CeilToInt(Mathf.Lerp(maxDamage, minDamageRatio * maxDamage, distance / (radius + tolerance)));
      TakeDamage(damage);
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
    ControlsManager.OnControlUp.AddListener(OnPlayerModeToggleRelease);
    ControlsManager.OnControlUp.AddListener(OnNextWeaponButtonReleased);

    smokeEffectSM.SetActive(false);
    smokeEffectMD.SetActive(false);
    smokeEffectLG.SetActive(false);
  }

  void SetInitialState()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = gs.maxPlayerHealth,
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
    state.energy += state.batteryRechargeRate * state.batteryCapacity;
    if (state.energy > state.batteryCapacity * state.batteryAmount) state.energy = state.batteryCapacity * state.batteryAmount;

    state.turnStage = PlayerState.TurnStage.Moving;
    UIManager.UpdateActiveBatteryBars(state.energy, state.batteryCapacity, state.batteryAmount, state.batteryRechargeRate);
    UIManager.UpdateActiveHealthBar(state.health, state.maxHealth);
    UIManager.SetTouchControlsStage(state.turnStage);
    bool canAfford = state.energy >= shootingScript.GetCurrentWeapon().batteryCost * state.batteryCapacity;
    UIManager.SetActiveWeapon(shootingScript.GetCurrentWeapon(), canAfford);
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
  }

  public void TakeDamage(int damage)
  {
    state.health -= damage;
    UIManager.UpdateActiveHealthBar(state.health, state.maxHealth);

    Vector3 popupPosition = transform.position + new Vector3(0, 1, transform.localPosition.z);
    GameObject damagePopup = Instantiate(UIManager.Instance.references.damagePopupPrefab, popupPosition, Quaternion.identity);
    float scaleFactor = 2f;
    damagePopup.transform.localScale = new Vector3(transform.localScale.x * scaleFactor, transform.localScale.y * scaleFactor, 1);
    damagePopup.GetComponent<TextMeshPro>().text = damage.ToString();

    UpdateSmokeFX();

    if (state.health <= 0)
    {
      if (state.isPlayingTurn) LevelManager.Instance.EndPlayerTurn();
      // TODO: Add explosion effect, particles and some climax to it
      Destroy(gameObject);
    }
  }

  // Event handlers
  // --------------------------------------------------

  void OnShootButtonReleased(ControlType controlType)
  {
    if (controlType != ControlType.Shoot || !state.isPlayingTurn) return;
    if (state.energy < shootingScript.GetCurrentWeapon().batteryCost * state.batteryCapacity) return;

    switch (state.turnStage)
    {
      case PlayerState.TurnStage.Moving:
      case PlayerState.TurnStage.Aiming:
      default:
        state.turnStage = PlayerState.TurnStage.Shooting;
        UIManager.SetTouchControlsStage(state.turnStage);
        UIManager.AnimateStrengthIndicator();
        if (lastShotStrengthValue != -1) UIManager.SetStrengthIndicatorGhostValue(lastShotStrengthValue);
        break;

      case PlayerState.TurnStage.Shooting:
        float strengthValue = UIManager.GetStrengthIndicatorValue();
        float shotStrength = Mathf.Lerp(LevelManager.Instance.gameplaySettings.minShotStrength, LevelManager.Instance.gameplaySettings.maxShotStrength, strengthValue);
        GameObject bullet = shootingScript.Shoot(shotStrength);

        state.energy -= shootingScript.GetCurrentWeapon().batteryCost * state.batteryCapacity;

        // Resets after end of player turn
        CameraManager.TrackObject(bullet); // Track bullet
        CameraManager.Zoom(false); // Zoom out
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

  void OnPlayerModeToggleRelease(ControlType controlType)
  {
    if (controlType == ControlType.PlayerModeToggle)
    {
      if (state.isPlayingTurn)
      {
        if (state.turnStage == PlayerState.TurnStage.Moving)
        {
          state.turnStage = PlayerState.TurnStage.Aiming;
          UIManager.SetTouchControlsStage(state.turnStage);
        }
        else if (state.turnStage == PlayerState.TurnStage.Aiming)
        {
          state.turnStage = PlayerState.TurnStage.Moving;
          UIManager.SetTouchControlsStage(state.turnStage);
        }
      }
    }
  }

  void OnNextWeaponButtonReleased(ControlType controlType)
  {
    if (controlType != ControlType.WeaponNext || !state.isPlayingTurn) return;
    shootingScript.SetNextWeapon();
    bool canAfford = state.energy >= shootingScript.GetCurrentWeapon().batteryCost * state.batteryCapacity;
    UIManager.SetActiveWeapon(shootingScript.GetCurrentWeapon(), canAfford);
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

  void UpdateSmokeFX()
  {
    float healthRatio = (float)state.health / state.maxHealth;
    float minSafeHealth = 0.5f;

    if (healthRatio >= minSafeHealth)
    {
      smokeEffectSM.SetActive(false);
      smokeEffectMD.SetActive(false);
      smokeEffectLG.SetActive(false);
    }
    else
    {
      // If under minSafeHealth, activate effects
      smokeEffectSM.SetActive(true); // Automatically
      if (healthRatio < 0.25f) smokeEffectMD.SetActive(true);
      if (healthRatio < 0.15f) smokeEffectLG.SetActive(true);
    }
  }
}