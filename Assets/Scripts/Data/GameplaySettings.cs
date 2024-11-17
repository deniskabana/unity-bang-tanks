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

  [Header("Round settings")]
  public int maxPlayerHealth = 100;
  public int maxFuelPerRound = 100;

  [Header("Active Indicator")]
  public bool enableActiveIndicator = true;
  public bool animateActiveIndicator = true;
}