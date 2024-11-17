using UnityEngine;

[System.Serializable]
public class GameplaySettings
{
  [SerializeField, Range(1, 8)] public int amountOfPlayers = 2;
  public enum PlayerPositioning { Random, Even }
  public PlayerPositioning playerPositioning = PlayerPositioning.Even;
  public int maxPlayerHealth = 100;
  public int maxFuelPerRound = 100;
  public float playerScale = 0.6f;
  public bool enableActiveIndicator = true;
  public bool animateActiveIndicator = true;
}