using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayersManager : MonoBehaviour
{
  [Header("Level Settings")]
  [SerializeField, Range(1, 8)] int amountOfPlayers = 2;

  [Header("Gameplay Customization Settings")]
  [SerializeField] bool limitedFuel = true;
  [SerializeField] float fuelPerRound = 100f;

  [Header("References")]
  [SerializeField] GameObject playerPrefab;

  // Private variables
  // --------------------------------------------------

  private List<GameObject> players = new List<GameObject>();
  private int currentPlayerIndex = 0;

  // Built-in methods
  // --------------------------------------------------

  void Start()
  {
    Initialize();
  }

  // Custom methods
  // --------------------------------------------------

  public void Initialize()
  {
    // This function should either retrieve or receive terrain data !!!
    CreatePlayers();
  }

  void CreatePlayers()
  {
    float[] playerPositions = new float[amountOfPlayers];

    // TODO: replace with actual value
    float textureWidth = 1920f;

    for (int i = 0; i < amountOfPlayers; i++)
    {
      float terrainPartSize = textureWidth / (amountOfPlayers + 2);
      playerPositions[i] = terrainPartSize * (i + 1);
      GameObject player = Instantiate(playerPrefab, new Vector3(playerPositions[i], 0, 0), Quaternion.identity);
      players.Add(player);
    }

    // Perform player initialization
    foreach (GameObject player in players)
    {
      // player.GetComponent<Player>().Initialize(limitedFuel, maxFuel, enableRoundTimer, maxRoundDuration);
    }
  }

  public int GetCurrentPlayerIndex()
  {
    return currentPlayerIndex;
  }

  public GameObject GetCurrentPlayerObject()
  {
    return players[currentPlayerIndex];
  }
}
