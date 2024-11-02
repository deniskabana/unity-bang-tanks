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
    private TerrainSettings terrainSettings;

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
        terrainSettings = _terrainSettings;

        if (debug) Debug.Log("Initializing terrain...");
        GenerateHeightmapData();
        GenerateHeightmapTexture();

        if (terrainSettings.enableTerrainChunking) CreateTerrainChunks();
        else CreateTerrainSingleChunk();
        initialized = true;

        if (debug) Debug.Log("Terrain initialized.");
    }

    void GenerateHeightmapData()
    {
        if (debug) Debug.Log("Generating heightmap data...");
        heightmap = new float[terrainSettings.textureWidth];
        float randomOffset = Random.Range(terrainSettings.minRandomOffset, terrainSettings.maxRandomOffset); // The range can be adjusted to vary the terrain more or less

        // Generate Perlin noise for each x position
        for (int x = 0; x < terrainSettings.textureWidth; x++)
        {
            float yHeight = Mathf.PerlinNoise(x * terrainSettings.noiseScale + randomOffset, 0) * terrainSettings.heightMultiplier;
            heightmap[x] = yHeight;
        }
        if (debug) Debug.Log("Heightmap data generated.");
    }

    void GenerateHeightmapTexture()
    {
        if (debug) Debug.Log("Generating heightmap texture...");
        heightmapTexture = new Texture2D(terrainSettings.textureWidth, terrainSettings.textureHeight, TextureFormat.RGBA32, false);
        Color[] pixelColorList = new Color[terrainSettings.textureWidth * terrainSettings.textureHeight];

        // Generate the heightmap texture based on the heightmap data
        for (int x = 0; x < terrainSettings.textureWidth; x++)
        {
            for (int y = 0; y < terrainSettings.textureHeight; y++)
            {
                if (y < heightmap[x])
                {
                    pixelColorList[y * terrainSettings.textureWidth + x] = Color.white;
                }
                else
                {
                    pixelColorList[y * terrainSettings.textureWidth + x] = Color.clear;
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
        renderer.size = new Vector2(terrainSettings.textureWidth / terrainSettings.pixelsPerUnit, terrainSettings.textureHeight / terrainSettings.pixelsPerUnit);
        renderer.bounds.SetMinMax(Vector3.zero, new Vector3(terrainSettings.textureWidth / terrainSettings.pixelsPerUnit, terrainSettings.textureHeight / terrainSettings.pixelsPerUnit, 0));
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
        int gridWidth = Mathf.CeilToInt((float)terrainSettings.textureWidth / terrainSettings.chunkSize);
        int gridHeight = Mathf.CeilToInt((float)terrainSettings.textureHeight / terrainSettings.chunkSize);

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
                bool isTerrain = false;

                for (int i = 0; i < terrainSettings.chunkSize; i++)
                {
                    if (xCursor * terrainSettings.chunkSize + i >= heightmap.Length) break;
                    if (yCursor * terrainSettings.chunkSize <= heightmap[xCursor * terrainSettings.chunkSize + i])
                    {
                        isTerrain = true;
                        break;
                    }
                }
                // Skip this iteration if the segment is not part of the terrain
                if (!isTerrain) continue;

                Vector3 chunkPosition = collidersGroupTransform.position;
                float chunkWorldSize = terrainSettings.chunkSize / terrainSettings.pixelsPerUnit;

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
            -terrainSettings.textureWidth / 2 / terrainSettings.pixelsPerUnit + terrainSettings.chunkSize / 2 / terrainSettings.pixelsPerUnit,
            -terrainSettings.textureHeight / 2 / terrainSettings.pixelsPerUnit + terrainSettings.chunkSize / 2 / terrainSettings.pixelsPerUnit,
            0
        );

        if (debug) Debug.Log("Terrain chunks created.");
    }

    float[] CreateChunkHeightmap(int xCursor, int yCursor)
    {
        float[] chunkHeightmap = new float[terrainSettings.chunkSize];
        int startX = xCursor * terrainSettings.chunkSize;

        for (int x = 0; x < terrainSettings.chunkSize; x++)
        {
            int worldX = startX + x;
            // If out of bounds, skip
            if (worldX >= terrainSettings.textureWidth) continue;
            chunkHeightmap[x] = heightmap[worldX] - yCursor * terrainSettings.chunkSize;
        }

        return chunkHeightmap;
    }

    Texture2D CreateChunkTexture(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(terrainSettings.chunkSize, terrainSettings.chunkSize, TextureFormat.RGBA32, false);
        gridSegmentTexture.filterMode = FilterMode.Point; // Ensure pixel-perfect sampling

        // Calculate the actual heightmap values for this segment
        Color[] pixels = new Color[terrainSettings.chunkSize * terrainSettings.chunkSize];

        int startX = xCursor * terrainSettings.chunkSize;
        int startY = yCursor * terrainSettings.chunkSize;

        for (int x = 0; x < terrainSettings.chunkSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= terrainSettings.textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < terrainSettings.chunkSize; y++)
            {
                int worldY = startY + y;

                // Basic fill
                bool isSolid = worldY < heightAtX;

                // Edge detection - are we on a segment border?
                bool isOnXBorder = x == 0 || x == terrainSettings.chunkSize - 1;
                bool isOnYBorder = y == 0 || y == terrainSettings.chunkSize - 1;

                if (isSolid)
                {
                    // If we're not on a border, it's definitely solid
                    if (!isOnXBorder && !isOnYBorder)
                    {
                        pixels[y * terrainSettings.chunkSize + x] = Color.white;
                    }
                    // If we're on a border, check neighboring cells
                    else
                    {
                        bool shouldBeSolid = true;

                        // Check neighboring heightmap values if we're on X border
                        if (isOnXBorder)
                        {
                            int neighborX = worldX + (x == 0 ? -1 : 1);
                            if (neighborX >= 0 && neighborX < terrainSettings.textureWidth)
                            {
                                float neighborHeight = heightmap[neighborX];
                                // Only be solid if the neighbor would also be solid at this height
                                shouldBeSolid = worldY < neighborHeight;
                            }
                        }

                        // If we're still solid after checks, set the pixel
                        if (shouldBeSolid)
                        {
                            pixels[y * terrainSettings.chunkSize + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * terrainSettings.chunkSize + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * terrainSettings.chunkSize + x] = Color.clear;
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
