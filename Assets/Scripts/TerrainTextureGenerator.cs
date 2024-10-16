using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] int textureWidth = 2048;  // The width of the terrain (in pixels)
    [SerializeField] int textureHeight = 1024; // The max height of the terrain (in pixels)
    [SerializeField] float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
    [SerializeField] float heightMultiplier = 600f; // Controls the maximum height

    [Header("References")]
    [SerializeField] GameObject gridParentObject;
    [SerializeField] GameObject heightmapMaskObject;
    [SerializeField] GameObject terrainGridPrefab;

    [Header("Advanced terrain settings")]
    [SerializeField] float pixelsPerUnit = 32f;
    [SerializeField] float minRandomOffset = 0; // The maximum random offset for Perlin noise
    [SerializeField] float maxRandomOffset = 1000f; // The maximum random offset for Perlin noise
    [SerializeField, Range(2, 512)] int terrainGridCellSize = 32; // Size in pixels by which we will subdivide the terrain

    // Private variables
    // --------------------------------------------------

    private float[] heightmap;
    private Texture2D heightmapTexture;

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
        GenerateHeightmapData();
        // Step 2: Generate a single big texture based on the heightmap
        GenerateHeightmapTexture();
        // Step 3: Create a grid of terrain segments based on the heightmap
        GenerateTerrainGridTextured();
    }

    void GenerateHeightmapData()
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

    void GenerateHeightmapTexture()
    {
        heightmapTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        Color[] pixelColorList = new Color[textureWidth * textureHeight];

        // Generate the heightmap texture based on the heightmap data
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                if (y < heightmap[x])
                {
                    pixelColorList[y * textureWidth + x] = Color.white;
                }
                else
                {
                    pixelColorList[y * textureWidth + x] = Color.clear;
                }
            }
        }

        heightmapTexture.SetPixels(pixelColorList);
        heightmapTexture.Apply();

        // Set the sprite for the heightmap mask object
        heightmapMaskObject.GetComponent<SpriteMask>().sprite = Sprite.Create(heightmapTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 1);

        // Ensure the heightmapMaskObject retains the same size and position as the texture
        heightmapMaskObject.transform.position = transform.position;

        // Adjust the scale to match the texture size
        float scaleX = textureWidth / heightmapMaskObject.GetComponent<SpriteMask>().sprite.bounds.size.x;
        float scaleY = textureHeight / heightmapMaskObject.GetComponent<SpriteMask>().sprite.bounds.size.y;

        // Adjust the scale to match the world units
        float pixelsPerUnit = 32f; // Adjust this value based on your project's settings
        heightmapMaskObject.transform.localScale = new Vector3(scaleX / pixelsPerUnit, scaleY / pixelsPerUnit, 1);

        // Adjust the position to account for the texture's width and height
        heightmapMaskObject.transform.position = new Vector3(
            transform.position.x + (textureWidth / pixelsPerUnit) / 2,
            transform.position.y + (textureHeight / pixelsPerUnit) / 2,
            transform.position.z
        );
    }

    void GenerateTerrainGridTextured()
    {
        if (terrainGridPrefab == null || heightmap == null || gridParentObject == null) return;

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

                // Position the grid segment to ensure no gaps
                Vector3 terrainGridPosition = gridParentObject.transform.position;
                Vector3 position = new Vector3(xCursor * segmentSize + terrainGridPosition.x, yCursor * segmentSize + terrainGridPosition.y, 0);

                // Instantiate the grid segment prefab
                GameObject gridSegment = Instantiate(terrainGridPrefab, position, Quaternion.identity, gridParentObject.transform);

                // Scale to match the segment size in world units (no extra scaling needed here)
                gridSegment.transform.localScale = new Vector3(1, 1, 1);

                // Create texture for the current segment and assign it

                // TODO: UNCOMMENT
                // Texture2D spriteForCurrentSegment = CreateTextureForSegment(xCursor, yCursor);
                // gridSegment.GetComponent<SpriteMask>().sprite = Sprite.Create(spriteForCurrentSegment, new Rect(0, 0, terrainGridCellSize, terrainGridCellSize), new Vector2(0.5f, 0.5f), terrainGridCellSize);
                // gridSegment.GetComponent<TerrainGridSegment>().UpdateCollider();
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
