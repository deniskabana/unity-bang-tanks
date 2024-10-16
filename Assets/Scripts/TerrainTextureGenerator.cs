using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] int textureWidth = 2048;  // The width of the terrain (in pixels)
    [SerializeField] int textureHeight = 1024; // The max height of the terrain (in pixels)
    [SerializeField] float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
    [SerializeField] float heightMultiplier = 600f; // Controls the maximum height

    [Header("References")]
    // [SerializeField] SpriteRenderer spriteRenderer; // The 2D SpriteRenderer to display the texture
    [SerializeField] GameObject terrainGridPrefab; // The 2D SpriteRenderer to display the texture

    [Header("Advanced terrain settings")]
    [SerializeField] float minRandomOffset = 0; // The maximum random offset for Perlin noise
    [SerializeField] float maxRandomOffset = 1000f; // The maximum random offset for Perlin noise
    [SerializeField, Range(2, 512)] int terrainGridCellSize = 32; // Size in pixels by which we will subdivide the terrain

    // Private variables
    // --------------------------------------------------

    private float[] heightmap;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        // Initialize();
    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        // Step 1: Generate the Perlin noise-based heightmap
        GenerateHeightmap();
        // Step 2: Render the heightmap as a texture
        GenerateTerrainGridTextured();
    }

    void GenerateHeightmap()
    {
        heightmap = new float[textureWidth];
        float randomOffset = Random.Range(minRandomOffset, maxRandomOffset); // The range can be adjusted to vary the terrain more or less

        // Generate Perlin noise for each x position
        for (int x = 0; x < textureWidth; x++)
        {
            float yHeight = Mathf.PerlinNoise(x * noiseScale + randomOffset, 0) * heightMultiplier;
            heightmap[x] = yHeight;
        }
    }

    void GenerateTerrainGridTextured()
    {
        if (terrainGridPrefab == null || heightmap == null)
        {
            Debug.LogError("TerrainTextureGenerator: Terrain grid prefab or heightmap not set!");
            return;
        }

        // Calculate the grid width and height based on the terrain grid cell size
        int gridWidth = Mathf.CeilToInt(textureWidth / terrainGridCellSize);
        int gridHeight = Mathf.CeilToInt(textureHeight / terrainGridCellSize);

        // Ensure the segments fit tightly together in world space
        float segmentSize = 1.0f; // Each segment is 1 world unit wide and high, assuming 1:1 ratio

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
                // Only continue if the current grid cell is part of the terrain by checking the heightmap
                bool isTerrain = false;

                for (int i = 0; i < terrainGridCellSize; i++)
                {
                    if (xCursor * terrainGridCellSize + i >= heightmap.Length) break;
                    if (yCursor * terrainGridCellSize <= heightmap[xCursor * terrainGridCellSize + i])
                    {
                        isTerrain = true;
                        break;
                    }
                }

                if (!isTerrain) continue;

                GameObject terrainGridParent = GameObject.Find("TerrainGrid");
                if (terrainGridParent == null) terrainGridParent = new GameObject("TerrainGrid");

                // Position the grid segment to ensure no gaps
                Vector3 terrainGridPosition = terrainGridParent.transform.position;
                Vector3 position = new Vector3(xCursor * segmentSize + terrainGridPosition.x, yCursor * segmentSize + terrainGridPosition.y, 0);

                // Instantiate the grid segment prefab
                GameObject gridSegment = Instantiate(terrainGridPrefab, position, Quaternion.identity, terrainGridParent.transform);

                // Scale to match the segment size in world units (no extra scaling needed here)
                gridSegment.transform.localScale = new Vector3(1, 1, 1);

                // Create texture for the current segment and assign it
                Texture2D spriteForCurrentSegment = CreateTextureForSegment(xCursor, yCursor);
                gridSegment.GetComponent<SpriteMask>().sprite = Sprite.Create(spriteForCurrentSegment, new Rect(0, 0, terrainGridCellSize, terrainGridCellSize), new Vector2(0.5f, 0.5f), terrainGridCellSize);

                // Update collider if needed
                gridSegment.GetComponent<TerrainGridSegment>().UpdateCollider();
            }
        }
    }

    Texture2D CreateTextureForSegment(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(terrainGridCellSize, terrainGridCellSize, TextureFormat.RGBA32, false);
        Color[] pixelColorList = new Color[terrainGridCellSize * terrainGridCellSize];

        // Generate the grid segment texture based on the heightmap
        for (int x = 0; x < terrainGridCellSize; x++)
        {
            for (int y = 0; y < terrainGridCellSize; y++)
            {
                int heightmapX = xCursor * terrainGridCellSize + x;
                int heightmapY = yCursor * terrainGridCellSize + y;

                // Ensure we are within the bounds of the heightmap
                if (heightmapX < heightmap.Length && heightmapY < heightmap[heightmapX])
                {
                    pixelColorList[y * terrainGridCellSize + x] = Color.white;
                }
                else
                {
                    pixelColorList[y * terrainGridCellSize + x] = Color.clear;
                }
            }
        }

        gridSegmentTexture.SetPixels(pixelColorList);
        gridSegmentTexture.Apply();
        return gridSegmentTexture;
    }

}
