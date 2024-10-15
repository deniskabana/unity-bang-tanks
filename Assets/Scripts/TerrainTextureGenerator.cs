using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] int textureWidth = 2048;  // The width of the terrain (in pixels)
    [SerializeField] int textureHeight = 1024; // The max height of the terrain (in pixels)
    [SerializeField] float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
    [SerializeField] float heightMultiplier = 600f; // Controls the maximum height

    [Header("References")]
    [SerializeField] SpriteRenderer spriteRenderer; // The 2D SpriteRenderer to display the texture
    [SerializeField] Texture2D highResTerrainTexture; // Assign the high-res texture here


    // Private variables
    // --------------------------------------------------

    private float[] heightmap;
    private Texture2D terrainTexture;
    private PolygonCollider2D terrainCollider;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        Initialize();
    }

    void OnDestroy()
    {
        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }
    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {

        // Step 1: Generate the Perlin noise-based heightmap
        GenerateHeightmap();
        // Step 2: Render the heightmap as a texture
        GenerateTexture();
        // Step 3: Update the collider to match the new terrain
        UpdateCollider();

        // Debug check the assigned sprite
        Debug.Log("Assigned Sprite: " + spriteRenderer.sprite.name);
    }

    void GenerateHeightmap()
    {
        heightmap = new float[textureWidth]; // Initialize the heightmap array

        // Introduce a random offset for the Perlin noise to ensure the terrain is different each time
        float randomOffset = Random.Range(0f, 1000f); // The range can be adjusted to vary the terrain more or less

        // Generate Perlin noise for each x position
        for (int x = 0; x < textureWidth; x++)
        {
            // Calculate the height based on Perlin noise
            float yHeight = Mathf.PerlinNoise(x * noiseScale + randomOffset, 0) * heightMultiplier;

            // Store the generated height in the heightmap
            heightmap[x] = yHeight;
        }
    }

    void GenerateTexture()
    {
        terrainTexture = new Texture2D(textureWidth, textureHeight);

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                Color pixelColor;

                // Check if the current pixel should be part of the terrain (ground) or sky (transparent)
                if (y <= heightmap[x])
                {
                    // Sample the high-res terrain texture instead of filling with green
                    float u = (float)x / textureWidth * 4;   // Normalized X coordinate (0 to 1)
                    float v = (float)y / textureHeight * 4;  // Normalized Y coordinate (0 to 1)

                    // Sample the high-res texture at (u, v)
                    if (highResTerrainTexture != null)
                    {
                        pixelColor = highResTerrainTexture.GetPixelBilinear(u, v); // Bilinear sampling for smoother results
                    }
                    else
                    {
                        // Fallback to green if the high-res texture is not assigned
                        pixelColor = new Color(0f, 1f, 0f, 1f);  // Opaque green
                    }
                }
                else
                {
                    // Transparent sky
                    pixelColor = new Color(1f, 1f, 1f, 0f);  // Fully transparent
                }

                terrainTexture.SetPixel(x, y, pixelColor);
            }
        }

        terrainTexture.Apply();  // Apply the changes to the texture

        // Debug: Check if the texture is sampled correctly
        Debug.Log("Texture generated. First pixel sampled from high-res texture: " + terrainTexture.GetPixel(0, 0));

        ApplyTextureToSpriteRenderer();  // Apply the texture as a sprite to the SpriteRenderer
    }

    void ApplyTextureToSpriteRenderer()
    {
        // Convert the texture to a Sprite and apply it to the SpriteRenderer
        if (terrainTexture == null)
        {
            Debug.LogError("No terrain texture generated!");
            return;
        }

        Sprite terrainSprite = Sprite.Create(terrainTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
        terrainSprite.name = "ProceduralTerrain"; // Name the sprite for easy identification
        spriteRenderer.sprite = terrainSprite;

        Debug.Log("Terrain texture applied to sprite renderer.");
    }

    void UpdateCollider()
    {
        if (!Application.isPlaying) return;

        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }

        terrainCollider = gameObject.AddComponent<PolygonCollider2D>();
    }
}
