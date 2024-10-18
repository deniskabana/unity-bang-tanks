using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] int textureWidth = 1920;  // The width of the terrain (in pixels)
    [SerializeField] int textureHeight = 1080; // The max height of the terrain (in pixels)
    [SerializeField] float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
    [SerializeField] float heightMultiplier = 600f; // Controls the maximum height

    [Header("References")]
    [SerializeField] GameObject gridParentObject;
    [SerializeField] GameObject heightmapMaskObject;
    [SerializeField] GameObject terrainGridPrefab;

    [Header("Advanced terrain settings")]
    [SerializeField] float pixelsPerUnit = 100f;
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
        GenerateTerrainGridColliders();
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
        SpriteRenderer heightmapSpriteRenderer = heightmapMaskObject.GetComponent<SpriteRenderer>();
        heightmapSpriteRenderer.material.SetTexture("_HeightMap", heightmapTexture);

        // Set the sprite renderer to the exact same bounds and size as the heightmapTexture
        heightmapSpriteRenderer.size = new Vector2(textureWidth / pixelsPerUnit, textureHeight / pixelsPerUnit);
        heightmapSpriteRenderer.bounds.SetMinMax(Vector3.zero, new Vector3(textureWidth / pixelsPerUnit, textureHeight / pixelsPerUnit, 0));
        heightmapSpriteRenderer.drawMode = SpriteDrawMode.Sliced;

        // Ensure the heightmapMaskObject retains the same size and position as the texture
        heightmapMaskObject.transform.position = gridParentObject.transform.position;
        heightmapMaskObject.transform.localScale = new Vector3(1, 1, 1);
    }

    void GenerateTerrainGridColliders()
    {
        if (terrainGridPrefab == null || heightmap == null || gridParentObject == null) return;

        // Calculate the grid width and height based on the terrain grid cell size
        int gridWidth = Mathf.CeilToInt(textureWidth / terrainGridCellSize);
        int gridHeight = Mathf.CeilToInt(textureHeight / terrainGridCellSize);

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
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

                // Skip this iteration if the segment is not part of the terrain
                if (!isTerrain) continue;

                Vector3 terrainGridPosition = gridParentObject.transform.position;
                float gridWorldSize = terrainGridCellSize / pixelsPerUnit;

                // For each grid segment, set its position in world space relative to the gridParentObject
                Vector3 position = new Vector3(
                    terrainGridPosition.x + xCursor * gridWorldSize,
                    terrainGridPosition.y + yCursor * gridWorldSize,
                    0);

                // Instantiate the grid segment prefab
                GameObject gridSegment = Instantiate(terrainGridPrefab, position, Quaternion.identity, gridParentObject.transform);
                gridSegment.transform.localScale = new Vector3(1, 1, 1);

                // Create texture for the current segment and assign it
                Texture2D spriteForCurrentSegment = CreateTextureForSegment(xCursor, yCursor);
                // spriteForCurrentSegment.filterMode = FilterMode.Point;
                Sprite segmentSprite = Sprite.Create(spriteForCurrentSegment, new Rect(0, 0, terrainGridCellSize, terrainGridCellSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                gridSegment.GetComponent<TerrainGridSegment>().UpdateCollider(segmentSprite);
            }
        }

        // Align the grid with the heightmap mask object
        gridParentObject.transform.position = new Vector3(
            -textureWidth / 2 / pixelsPerUnit + terrainGridCellSize / 2 / pixelsPerUnit,
            -textureHeight / 2 / pixelsPerUnit + terrainGridCellSize / 2 / pixelsPerUnit,
            0);
    }

    Texture2D CreateTextureForSegment(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(terrainGridCellSize, terrainGridCellSize, TextureFormat.RGBA32, false);

        // Copy the pixels from the heightmapTexture to the grid segment texture
        Color[] pixels = heightmapTexture.GetPixels(
            xCursor * terrainGridCellSize,
            yCursor * terrainGridCellSize,
            terrainGridCellSize,
            terrainGridCellSize
        );

        gridSegmentTexture.SetPixels(pixels);
        gridSegmentTexture.Apply();
        return gridSegmentTexture;
    }
}
