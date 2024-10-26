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
        heightmapSpriteRenderer.enabled = true;
        heightmapSpriteRenderer.material.SetTexture("_HeightMap", heightmapTexture);
        SetMaterialTextureTiling(heightmapSpriteRenderer.material, heightmapTexture, heightmapSpriteRenderer.sprite.texture);

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

                // For each grid segment, set its position in world space relative to the TerrainGridGroup object
                Vector3 position = new Vector3(
                    terrainGridPosition.x + xCursor * gridWorldSize,
                    terrainGridPosition.y + yCursor * gridWorldSize,
                    0);

                // Instantiate the grid segment prefab
                TerrainChunk gridSegmentScript = Instantiate(terrainGridPrefab, position, Quaternion.identity, gridParentObject.transform)
                    .GetComponent<TerrainChunk>();
                TerrainChunkData terrainChunkData = new TerrainChunkData
                {
                    PixelsPerUnit = pixelsPerUnit,
                    ChunkHeightmapGrid = CreateChunkHeightmap(xCursor, yCursor),
                    TerrainGridCellSize = terrainGridCellSize,
                    ChunkHeightmapTexture = CreateTextureForSegment(xCursor, yCursor)
                };

                // Initialize the terrain chunk
                gridSegmentScript.Initialize(terrainChunkData);
            }
        }

        // Align the grid with the heightmap mask object
        gridParentObject.transform.position = new Vector3(
            -textureWidth / 2 / pixelsPerUnit + terrainGridCellSize / 2 / pixelsPerUnit,
            -textureHeight / 2 / pixelsPerUnit + terrainGridCellSize / 2 / pixelsPerUnit,
            0);
    }

    bool[,] CreateChunkHeightmap(int xCursor, int yCursor)
    {
        bool[,] chunkHeightmap = new bool[terrainGridCellSize, terrainGridCellSize];

        int startX = xCursor * terrainGridCellSize;
        int startY = yCursor * terrainGridCellSize;

        for (int x = 0; x < terrainGridCellSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < terrainGridCellSize; y++)
            {
                int worldY = startY + y;
                chunkHeightmap[x, y] = worldY < heightAtX;
            }
        }

        return chunkHeightmap;
    }

    Texture2D CreateTextureForSegment(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(terrainGridCellSize, terrainGridCellSize, TextureFormat.RGBA32, false);
        gridSegmentTexture.filterMode = FilterMode.Point; // Ensure pixel-perfect sampling

        // Calculate the actual heightmap values for this segment
        Color[] pixels = new Color[terrainGridCellSize * terrainGridCellSize];

        int startX = xCursor * terrainGridCellSize;
        int startY = yCursor * terrainGridCellSize;

        for (int x = 0; x < terrainGridCellSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < terrainGridCellSize; y++)
            {
                int worldY = startY + y;

                // Basic fill
                bool isSolid = worldY < heightAtX;

                // Edge detection - are we on a segment border?
                bool isOnXBorder = x == 0 || x == terrainGridCellSize - 1;
                bool isOnYBorder = y == 0 || y == terrainGridCellSize - 1;

                if (isSolid)
                {
                    // If we're not on a border, it's definitely solid
                    if (!isOnXBorder && !isOnYBorder)
                    {
                        pixels[y * terrainGridCellSize + x] = Color.white;
                    }
                    // If we're on a border, check neighboring cells
                    else
                    {
                        bool shouldBeSolid = true;

                        // Check neighboring heightmap values if we're on X border
                        if (isOnXBorder)
                        {
                            int neighborX = worldX + (x == 0 ? -1 : 1);
                            if (neighborX >= 0 && neighborX < textureWidth)
                            {
                                float neighborHeight = heightmap[neighborX];
                                // Only be solid if the neighbor would also be solid at this height
                                shouldBeSolid = worldY < neighborHeight;
                            }
                        }

                        // If we're still solid after checks, set the pixel
                        if (shouldBeSolid)
                        {
                            pixels[y * terrainGridCellSize + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * terrainGridCellSize + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * terrainGridCellSize + x] = Color.clear;
                }
            }
        }

        gridSegmentTexture.SetPixels(pixels);
        gridSegmentTexture.Apply();
        return gridSegmentTexture;
    }

    void SetMaterialTextureTiling(Material material, Texture2D heightmapTexture, Texture2D texture)
    {
        // Get the texture width and height
        float textureWidth = texture.width;
        float textureHeight = texture.height;

        // Calculate the tiling factors based on terrain size and texture size
        float tilingX = heightmapTexture.width / textureWidth;
        float tilingY = heightmapTexture.height / textureHeight;

        // Set the tiling factors in the material (for MainTex only)
        material.SetFloat("_TilingX", tilingX);
        material.SetFloat("_TilingY", tilingY);
    }
}
