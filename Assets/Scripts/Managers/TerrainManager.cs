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
    private TerrainSettings ts;

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
        ts = _terrainSettings;

        if (debug) Debug.Log("Initializing terrain...");
        GenerateHeightmapData();
        GenerateHeightmapTexture();

        if (ts.enableTerrainChunking) CreateTerrainChunks();
        else CreateTerrainSingleChunk();
        initialized = true;

        if (debug) Debug.Log("Terrain initialized.");
    }

    void GenerateHeightmapData()
    {
        if (debug) Debug.Log("Generating heightmap data...");
        heightmap = new float[ts.textureWidth];
        float randomOffset = Random.Range(ts.minRandomOffset, ts.maxRandomOffset); // The range can be adjusted to vary the terrain more or less

        // Generate Perlin noise for each x position
        for (int x = 0; x < ts.textureWidth; x++)
        {
            float yHeight = Mathf.PerlinNoise(x * ts.noiseScale + randomOffset, 0) * ts.heightMultiplier;
            heightmap[x] = yHeight;
        }
        if (debug) Debug.Log("Heightmap data generated.");
    }

    void GenerateHeightmapTexture()
    {
        if (debug) Debug.Log("Generating heightmap texture...");
        heightmapTexture = new Texture2D(ts.textureWidth, ts.textureHeight, TextureFormat.RGBA32, false);
        Color[] pixelColorList = new Color[ts.textureWidth * ts.textureHeight];

        // Generate the heightmap texture based on the heightmap data
        for (int x = 0; x < ts.textureWidth; x++)
        {
            for (int y = 0; y < ts.textureHeight; y++)
            {
                if (y < heightmap[x])
                {
                    pixelColorList[y * ts.textureWidth + x] = Color.white;
                }
                else
                {
                    pixelColorList[y * ts.textureWidth + x] = Color.clear;
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
        renderer.size = new Vector2(ts.textureWidth / ts.pixelsPerUnit, ts.textureHeight / ts.pixelsPerUnit);
        renderer.bounds.SetMinMax(Vector3.zero, new Vector3(ts.textureWidth / ts.pixelsPerUnit, ts.textureHeight / ts.pixelsPerUnit, 0));
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
        int gridWidth = Mathf.CeilToInt((float)ts.textureWidth / ts.chunkSize);
        int gridHeight = Mathf.CeilToInt((float)ts.textureHeight / ts.chunkSize);

        for (int xCursor = 0; xCursor < gridWidth; xCursor++)
        {
            for (int yCursor = 0; yCursor < gridHeight; yCursor++)
            {
                bool isTerrain = false;

                for (int i = 0; i < ts.chunkSize; i++)
                {
                    if (xCursor * ts.chunkSize + i >= heightmap.Length) break;
                    if (yCursor * ts.chunkSize <= heightmap[xCursor * ts.chunkSize + i])
                    {
                        isTerrain = true;
                        break;
                    }
                }
                // Skip this iteration if the segment is not part of the terrain
                if (!isTerrain) continue;

                Vector3 chunkPosition = collidersGroupTransform.position;
                float chunkWorldSize = ts.chunkSize / ts.pixelsPerUnit;

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
            -ts.textureWidth / 2 / ts.pixelsPerUnit + ts.chunkSize / 2 / ts.pixelsPerUnit,
            -ts.textureHeight / 2 / ts.pixelsPerUnit + ts.chunkSize / 2 / ts.pixelsPerUnit,
            0
        );

        if (debug) Debug.Log("Terrain chunks created.");
    }

    float[] CreateChunkHeightmap(int xCursor, int yCursor)
    {
        float[] chunkHeightmap = new float[ts.chunkSize];
        int startX = xCursor * ts.chunkSize;

        for (int x = 0; x < ts.chunkSize; x++)
        {
            int worldX = startX + x;
            // If out of bounds, skip
            if (worldX >= ts.textureWidth) continue;
            chunkHeightmap[x] = heightmap[worldX] - yCursor * ts.chunkSize;
        }

        return chunkHeightmap;
    }

    Texture2D CreateChunkTexture(int xCursor, int yCursor)
    {
        Texture2D gridSegmentTexture = new Texture2D(ts.chunkSize, ts.chunkSize, TextureFormat.RGBA32, false);
        gridSegmentTexture.filterMode = FilterMode.Point; // Ensure pixel-perfect sampling

        // Calculate the actual heightmap values for this segment
        Color[] pixels = new Color[ts.chunkSize * ts.chunkSize];

        int startX = xCursor * ts.chunkSize;
        int startY = yCursor * ts.chunkSize;

        for (int x = 0; x < ts.chunkSize; x++)
        {
            int worldX = startX + x;
            if (worldX >= ts.textureWidth) continue;

            float heightAtX = heightmap[worldX];

            for (int y = 0; y < ts.chunkSize; y++)
            {
                int worldY = startY + y;

                // Basic fill
                bool isSolid = worldY < heightAtX;

                // Edge detection - are we on a segment border?
                bool isOnXBorder = x == 0 || x == ts.chunkSize - 1;
                bool isOnYBorder = y == 0 || y == ts.chunkSize - 1;

                if (isSolid)
                {
                    // If we're not on a border, it's definitely solid
                    if (!isOnXBorder && !isOnYBorder)
                    {
                        pixels[y * ts.chunkSize + x] = Color.white;
                    }
                    // If we're on a border, check neighboring cells
                    else
                    {
                        bool shouldBeSolid = true;

                        // Check neighboring heightmap values if we're on X border
                        if (isOnXBorder)
                        {
                            int neighborX = worldX + (x == 0 ? -1 : 1);
                            if (neighborX >= 0 && neighborX < ts.textureWidth)
                            {
                                float neighborHeight = heightmap[neighborX];
                                // Only be solid if the neighbor would also be solid at this height
                                shouldBeSolid = worldY < neighborHeight;
                            }
                        }

                        // If we're still solid after checks, set the pixel
                        if (shouldBeSolid)
                        {
                            pixels[y * ts.chunkSize + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * ts.chunkSize + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * ts.chunkSize + x] = Color.clear;
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
