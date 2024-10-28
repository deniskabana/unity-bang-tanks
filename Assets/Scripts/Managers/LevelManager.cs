using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField, Range(1, 6)] int amountOfPlayers = 2;

    // [Header("Gameplay Customization Settings")]
    // [SerializeField] bool limitedFuel = true;
    // [SerializeField] float maxFuel = 100f;
    // [SerializeField] bool enableRoundTimer = true;
    // [SerializeField] float maxRoundDuration = 60f;

    [Header("References")]
    [SerializeField] GameObject playerPrefab;

    // Private variables
    // --------------------------------------------------

    public List<GameObject> players = new List<GameObject>();

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
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

    public void StartTurn()
    { }

    public void EndTurn()
    { }

    public void GetCurrentPlayerTank()
    { }
}
