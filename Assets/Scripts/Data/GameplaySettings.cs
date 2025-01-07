using UnityEngine;

[System.Serializable]
public class GameplaySettings
{
  [Header("Players")]
  [SerializeField, Range(1, 8)] public int amountOfPlayers = 2;
  public enum PlayerPositioning { Random, Even }
  public PlayerPositioning playerPositioning = PlayerPositioning.Even;
  public float playerScale = 0.6f;
  public float minShotStrength = 200f;
  public float maxShotStrength = 1300f;

  [Header("Tank settings")]
  [SerializeField, Range(10, 200)] public int batteryCapacity = 25;
  [SerializeField, Range(1, 7)] public int batteryRechargeRate = 1;
  [SerializeField, Range(1, 7)] public int batteryAmountMax = 7;
  [SerializeField, Range(1, 7)] public int batteryAmountStart = 3;
  public float energyDepletionRate = 8f;
  public int maxPlayerHealth = 100;

  [Header("Active Indicator")]
  public bool enableActiveIndicator = true;
  public bool animateActiveIndicator = true;
}