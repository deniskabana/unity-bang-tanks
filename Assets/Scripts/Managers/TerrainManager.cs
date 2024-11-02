using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;
    public bool debug = false;

    [Header("References")]
    [SerializeField] Transform collidersGroupTransform;
    [SerializeField] GameObject terrainTextureObject;
    [SerializeField] GameObject terrainChunkPrefab;

    // Runtime variables
    // --------------------------------------------------

    private bool initialized = false;
    private float[] heightmap;
    private Texture2D heightmapTexture;
    private TerrainSettings tSet;

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize(TerrainSettings _terrainSettings)
    {
        if (initialized) return;
        tSet = _terrainSettings;

        if (debug) Debug.Log("Initializing terrain...");
        GenerateHeightmapData();
        GenerateHeightmapTexture();

        if (tSet.enableTerrainChunking) CreateTerrainChunks();
        else CreateTerrainSingleChunk();
        initialized = true;

        if (debug) Debug.Log("Terrain initialized.");
    }

    void GenerateHeightmapData()
    {
        if (debug) Debug.Log("Generating heightmap data...");

        heightmap = new float[tSet.textureWidth];
        float randomOffset = Random.Range(tSet.minRandomOffset, tSet.maxRandomOffset); // The range can be adjusted to vary the terrain more or less

        // Generate Perlin noise for each x position
        for (int x = 0; x < tSet.textureWidth; x++)
        {
            float yHeight = Mathf.PerlinNoise(x * tSet.noiseScale + randomOffset, 0) * tSet.heightMultiplier;
            heightmap[x] = yHeight;
        }
        if (debug) Debug.Log("Heightmap data generated.");
    }

    void GenerateHeightmapTexture()
    {
        if (debug) Debug.Log("Generating heightmap texture...");
        heightmapTexture = new Texture2D(tSet.textureWidth, tSet.textureHeight, TextureFormat.RGBA32, false);
        Color[] pixelColorList = new Color[tSet.textureWidth * tSet.textureHeight];

        // Generate the heightmap texture based on the heightmap data
        for (int x = 0; x < tSet.textureWidth; x++)
        {
            for (int y = 0; y < tSet.textureHeight; y++)
            {
                if (y < heightmap[x])
                {
                    pixelColorList[y * tSet.textureWidth + x] = Color.white;
                }
                else
                {
                    pixelColorList[y * tSet.textureWidth + x] = Color.clear;
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
        renderer.size = new Vector2(tSet.textureWidth, tSet.textureHeight);
        renderer.bounds.SetMinMax(Vector3.zero, new Vector3(tSet.textureWidth, tSet.textureHeight, 0));
        renderer.drawMode = SpriteDrawMode.Sliced;

        // Ensure the heightmapMaskObject retains the same size and position as the texture
        terrainTextureObject.transform.position = collidersGroupTransform.position;
        terrainTextureObject.transform.localScale = new Vector3(1, 1, 1);

        if (debug) Debug.Log("Heightmap texture generated.");
    }

    void CreateTerrainSingleChunk()
    {
        if (debug) Debug.Log("Creating single terrain chunk...");
        if (terrainChunkPrefab == null || heightmap == null || collidersGroupTransform == null)
            throw new System.Exception("TerrainManager: Missing required references for terrain chunk generation.");

        // Instantiate the grid segment prefab
        GameObject chunkObject = Instantiate(terrainChunkPrefab, collidersGroupTransform.position, Quaternion.identity, collidersGroupTransform);
        chunkObject.name = "TerrainSingleCollider";

        TerrainChunk chunkScript = chunkObject.GetComponent<TerrainChunk>();

        // Initialize the terrain chunk
        chunkScript.Initialize(heightmapTexture);
        if (debug) Debug.Log("Single terrain chunk created.");
    }

    void CreateTerrainChunks()
    {
        if (debug) Debug.Log("Creating terrain chunks...");
        if (terrainChunkPrefab == null || heightmap == null || collidersGroupTransform == null)
            throw new System.Exception("TerrainManager: Missing required references for terrain chunk generation.");

        // Calculate the grid width and height based on the terrain grid cell size
        int gridWidth = Mathf.CeilToInt((float)tSet.textureWidth / tSet.chunkSize);
        int gridHeight = Mathf.CeilToInt((float)tSet.textureHeight / tSet.chunkSize);

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
                bool isTerrain = false;

                for (int i = 0; i < tSet.chunkSize; i++)
                {
                    if (xCursor * tSet.chunkSize + i >= heightmap.Length) break;
                    if (yCursor * tSet.chunkSize <= heightmap[xCursor * tSet.chunkSize + i])
                    {
                        isTerrain = true;
                        break;
                    }
                }
                // Skip this iteration if the segment is not part of the terrain
                if (!isTerrain) continue;

                Vector3 chunkPosition = collidersGroupTransform.position;
                float chunkWorldSize = tSet.chunkSize;

                // For each chunk, set its position in world space relative to the parent object
                Vector3 position = new Vector3(
                    chunkPosition.x + xCursor * chunkWorldSize,
                    chunkPosition.y + yCursor * chunkWorldSize,
                    0);

                // Instantiate the grid segment prefab and initialize it
                GameObject chunkObject = Instantiate(terrainChunkPrefab, position, Quaternion.identity, collidersGroupTransform);
                chunkObject.name = $"TerrainChunk_x:{xCursor}_y:{yCursor}";
                TerrainChunk chunkScript = chunkObject.GetComponent<TerrainChunk>();
                chunkScript.Initialize(CreateChunkTexture(xCursor, yCursor));
            }
        }

        // Align the grid with the heightmap mask object
        collidersGroupTransform.position = new Vector3(
            -tSet.textureWidth / 2 + tSet.chunkSize / 2,
            -tSet.textureHeight / 2 + tSet.chunkSize / 2,
            0
        );

        if (debug) Debug.Log("Terrain chunks created.");
    }

    float[] CreateChunkHeightmap(int xCursor, int yCursor)
    {
        float[] chunkHeightmap = new float[tSet.chunkSize];
        int startX = xCursor * tSet.chunkSize;

        for (int x = 0; x < tSet.chunkSize; x++)
        {
            int worldX = startX + x;
            // If out of bounds, skip
            if (worldX >= tSet.textureWidth) continue;
            chunkHeightmap[x] = heightmap[worldX] - yCursor * tSet.chunkSize;
        }

        return chunkHeightmap;
    }

    Texture2D CreateChunkTexture(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(tSet.chunkSize, tSet.chunkSize, TextureFormat.RGBA32, false);
        gridSegmentTexture.filterMode = FilterMode.Point; // Ensure pixel-perfect sampling

        // Calculate the actual heightmap values for this segment
        Color[] pixels = new Color[tSet.chunkSize * tSet.chunkSize];

        int startX = xCursor * tSet.chunkSize;
        int startY = yCursor * tSet.chunkSize;

        for (int x = 0; x < tSet.chunkSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= tSet.textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < tSet.chunkSize; y++)
            {
                int worldY = startY + y;

                // Basic fill
                bool isSolid = worldY < heightAtX;

                // Edge detection - are we on a segment border?
                bool isOnXBorder = x == 0 || x == tSet.chunkSize - 1;
                bool isOnYBorder = y == 0 || y == tSet.chunkSize - 1;

                if (isSolid)
                {
                    // If we're not on a border, it's definitely solid
                    if (!isOnXBorder && !isOnYBorder)
                    {
                        pixels[y * tSet.chunkSize + x] = Color.white;
                    }
                    // If we're on a border, check neighboring cells
                    else
                    {
                        bool shouldBeSolid = true;

                        // Check neighboring heightmap values if we're on X border
                        if (isOnXBorder)
                        {
                            int neighborX = worldX + (x == 0 ? -1 : 1);
                            if (neighborX >= 0 && neighborX < tSet.textureWidth)
                            {
                                float neighborHeight = heightmap[neighborX];
                                // Only be solid if the neighbor would also be solid at this height
                                shouldBeSolid = worldY < neighborHeight;
                            }
                        }

                        // If we're still solid after checks, set the pixel
                        if (shouldBeSolid)
                        {
                            pixels[y * tSet.chunkSize + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * tSet.chunkSize + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * tSet.chunkSize + x] = Color.clear;
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

    public Bounds GetTerrainRendererBounds()
    {
        if (!initialized) throw new System.Exception("TerrainManager: Terrain not initialized yet.");
        return terrainTextureObject.GetComponent<SpriteRenderer>().bounds;
    }
}
