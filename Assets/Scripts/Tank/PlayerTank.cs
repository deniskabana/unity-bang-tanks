using System;
using UnityEngine;

[System.Serializable]
public struct PlayerAttributes
{
  [Header("Movement Settings")]
  public float speed;
  public float downhillSpeedMultiplier;
  public float gravityForce;
  public float maxSlopeAngle;

  [Header("Gameplay Round Settings")]
  public int maxHealth;
  public int maxFuel;

  // Shooting, weapon choice, etc.
  // TODO: prepare for multiple weapons
}

[System.Serializable]
public struct PlayerState
{
  [Header("Runtime Variables")]
  public bool isPlayingTurn;
  public int health;
  public int fuel;
  public float cannonAngle;

  // Shooting, weapon choice, etc.
  // TODO: prepare for multiple weapons
}

public class PlayerTank : MonoBehaviour
{
  // Runtime variables
  // --------------------------------------------------

  private PlayerAttributes attributes;
  private PlayerState state;

  // Built-in methods
  // --------------------------------------------------

  void Update()
  {
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize(PlayerAttributes _attributes)
  {
    attributes = _attributes;
    SetInitialState();

    // Event listeners
    LevelManager.OnPlayerTurnStart.AddListener(SetStateBeforeTurn);
    LevelManager.OnPlayerTurnEnd.AddListener(SetStateAfterTurn);
  }

  void SetInitialState()
  {
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = attributes.maxHealth,
      fuel = attributes.maxFuel,
      cannonAngle = 0f
    };
  }

  void SetStateBeforeTurn()
  {
    state.isPlayingTurn = true;
    state.fuel = attributes.maxFuel;
  }

  void SetStateAfterTurn()
  {
    state.isPlayingTurn = false;
  }

  // Events
  // --------------------------------------------------
}