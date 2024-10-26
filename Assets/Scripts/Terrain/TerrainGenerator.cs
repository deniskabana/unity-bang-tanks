using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] int textureWidth = 1920;  // The width of the terrain (in pixels)
    [SerializeField] int textureHeight = 1080; // The max height of the terrain (in pixels)
    [SerializeField] float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
    [SerializeField] float heightMultiplier = 600f; // Controls the maximum height

    [Header("References")]
    [SerializeField] Transform collidersGroupTransform;
    [SerializeField] GameObject terrainTextureObject;
    [SerializeField] GameObject terrainChunkPrefab;

    [Header("Advanced terrain settings")]
    [SerializeField] bool enableTerrainChunking = true;
    [SerializeField] float pixelsPerUnit = 100f;
    [SerializeField] float minRandomOffset = 0; // The maximum random offset for Perlin noise
    [SerializeField] float maxRandomOffset = 1000f; // The maximum random offset for Perlin noise
    [SerializeField, Range(2, 2048)] int chunkSize = 256; // Size in pixels by which we will subdivide the terrain

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

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        // Step 1: Generate the Perlin noise-based float[] heightmap
        GenerateHeightmapData();

        // Step 2: Generate and apply the texture
        GenerateHeightmapTexture();

        // Step 3: Split terrain into chunks (for collision detection)
        if (enableTerrainChunking) CreateTerrainChunks();
        else CreateTerrainSingleChunk();
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
        SpriteRenderer renderer = terrainTextureObject.GetComponent<SpriteRenderer>();
        renderer.enabled = true;
        renderer.material.SetTexture("_HeightMap", heightmapTexture); // Shader needs to know about the heightmap texture
        SetMaterialTextureTiling(renderer.material, heightmapTexture, renderer.sprite.texture); // Set the tiling factors

        // Set the sprite renderer to the exact same bounds and size as the heightmapTexture
        renderer.size = new Vector2(textureWidth / pixelsPerUnit, textureHeight / pixelsPerUnit);
        renderer.bounds.SetMinMax(Vector3.zero, new Vector3(textureWidth / pixelsPerUnit, textureHeight / pixelsPerUnit, 0));
        renderer.drawMode = SpriteDrawMode.Sliced;

        // Ensure the heightmapMaskObject retains the same size and position as the texture
        terrainTextureObject.transform.position = collidersGroupTransform.position;
        terrainTextureObject.transform.localScale = new Vector3(1, 1, 1);
    }

    void CreateTerrainSingleChunk()
    {
        if (terrainChunkPrefab == null || heightmap == null || collidersGroupTransform == null)
            throw new System.Exception("TerrainGenerator: Missing required references for terrain chunk generation.");

        // Instantiate the grid segment prefab
        GameObject chunkObject = Instantiate(terrainChunkPrefab, collidersGroupTransform.position, Quaternion.identity, collidersGroupTransform);
        chunkObject.name = "TerrainSingleCollider";

        TerrainChunk chunkScript = chunkObject.GetComponent<TerrainChunk>();
        TerrainChunkData chunkData = new TerrainChunkData
        {
            PixelsPerUnit = pixelsPerUnit,
            ChunkSize = textureWidth,
            Heightmap = heightmap,
            HeightmapTexture = heightmapTexture
        };

        // Initialize the terrain chunk
        chunkScript.Initialize(chunkData);
    }

    void CreateTerrainChunks()
    {
        if (terrainChunkPrefab == null || heightmap == null || collidersGroupTransform == null)
            throw new System.Exception("TerrainGenerator: Missing required references for terrain chunk generation.");

        // Calculate the grid width and height based on the terrain grid cell size
        int gridWidth = Mathf.CeilToInt((float)textureWidth / chunkSize);
        int gridHeight = Mathf.CeilToInt((float)textureHeight / chunkSize);

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
                bool isTerrain = false;

                for (int i = 0; i < chunkSize; i++)
                {
                    if (xCursor * chunkSize + i >= heightmap.Length) break;
                    if (yCursor * chunkSize <= heightmap[xCursor * chunkSize + i])
                    {
                        isTerrain = true;
                        break;
                    }
                }
                // Skip this iteration if the segment is not part of the terrain
                if (!isTerrain) continue;

                Vector3 chunkPosition = collidersGroupTransform.position;
                float chunkWorldSize = chunkSize / pixelsPerUnit;

                // For each chunk, set its position in world space relative to the parent object
                Vector3 position = new Vector3(
                    chunkPosition.x + xCursor * chunkWorldSize,
                    chunkPosition.y + yCursor * chunkWorldSize,
                    0);

                // Instantiate the grid segment prefab
                GameObject chunkObject = Instantiate(terrainChunkPrefab, position, Quaternion.identity, collidersGroupTransform);
                chunkObject.name = $"TerrainChunk_{xCursor}_{yCursor}";

                TerrainChunk chunkScript = chunkObject.GetComponent<TerrainChunk>();
                TerrainChunkData chunkData = new TerrainChunkData
                {
                    PixelsPerUnit = pixelsPerUnit,
                    ChunkSize = chunkSize,
                    Heightmap = CreateChunkHeightmap(xCursor, yCursor),
                    HeightmapTexture = CreateChunkTexture(xCursor, yCursor)

                };

                // Initialize the terrain chunk
                chunkScript.Initialize(chunkData);
            }
        }

        // Align the grid with the heightmap mask object
        collidersGroupTransform.position = new Vector3(
            -textureWidth / 2 / pixelsPerUnit + chunkSize / 2 / pixelsPerUnit,
            -textureHeight / 2 / pixelsPerUnit + chunkSize / 2 / pixelsPerUnit,
            0
        );
    }

    float[] CreateChunkHeightmap(int xCursor, int yCursor)
    {
        float[] chunkHeightmap = new float[chunkSize];
        int startX = xCursor * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = startX + x;
            // If out of bounds, skip
            if (worldX >= textureWidth) continue;
            chunkHeightmap[x] = heightmap[worldX] - yCursor * chunkSize;
        }

        return chunkHeightmap;
    }

    // Create a slice of the heightmap texture for a single chunk
    Texture2D CreateChunkTexture(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, false);
        gridSegmentTexture.filterMode = FilterMode.Point; // Ensure pixel-perfect sampling

        // Calculate the actual heightmap values for this segment
        Color[] pixels = new Color[chunkSize * chunkSize];

        int startX = xCursor * chunkSize;
        int startY = yCursor * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < chunkSize; y++)
            {
                int worldY = startY + y;

                // Basic fill
                bool isSolid = worldY < heightAtX;

                // Edge detection - are we on a segment border?
                bool isOnXBorder = x == 0 || x == chunkSize - 1;
                bool isOnYBorder = y == 0 || y == chunkSize - 1;

                if (isSolid)
                {
                    // If we're not on a border, it's definitely solid
                    if (!isOnXBorder && !isOnYBorder)
                    {
                        pixels[y * chunkSize + x] = Color.white;
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
                            pixels[y * chunkSize + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * chunkSize + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * chunkSize + x] = Color.clear;
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
