using System;
using UnityEngine;

[System.Serializable]
public struct PlayerState
{
  public bool isPlayingTurn;
  public int health;
  public int fuel;
  public float cannonAngle;

  // Shooting, weapon choice, etc.
  // TODO: prepare for multiple weapons
}

[@RequireComponent(typeof(TankPhysics))]
[@RequireComponent(typeof(TankShooting))]
[@RequireComponent(typeof(TankHealth))]
public class PlayerTank : MonoBehaviour
{
  // Runtime variables
  // --------------------------------------------------

  private PlayerState state;

  // Built-in methods
  // --------------------------------------------------

  void Update()
  {
    if (!state.isPlayingTurn) return;
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize()
  {
    SetInitialState();
  }

  void SetInitialState()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state = new PlayerState
    {
      isPlayingTurn = false,
      health = gs.maxPlayerHealth,
      fuel = gs.maxFuelPerRound,
      cannonAngle = 0f
    };
  }

  public void HandleTurnStart()
  {
    GameplaySettings gs = LevelManager.Instance.gameplaySettings;
    state.isPlayingTurn = true;
    state.fuel = gs.maxFuelPerRound;
  }

  public void HandleTurnEnd()
  {
    state.isPlayingTurn = false;
  }
}