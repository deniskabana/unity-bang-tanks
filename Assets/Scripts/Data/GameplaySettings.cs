using UnityEngine;

[System.Serializable]
public class GameplaySettings
{
  [SerializeField, Range(1, 8)] public int amountOfPlayers = 2;
  [SerializeField] public int maxPlayerHealth = 100;
  [SerializeField] public int maxFuelPerRound = 100;
  [SerializeField] public float playerScale = 0.6f;
}