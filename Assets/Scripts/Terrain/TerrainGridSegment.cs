using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
public class TerrainGridSegment : MonoBehaviour
{
    [SerializeField] private float minSolidChunkThreshold = 0.005f;

    // Private fields
    // --------------------------------------------------

    private bool wasInitialized = false;
    private TerrainChunkData data;
    private PolygonCollider2D terrainCollider;
    private SpriteRenderer spriteRenderer;

    // Built-in methods
    // --------------------------------------------------

    void OnDestroy()
    {
        DestroyCollider();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize(TerrainChunkData newData)
    {
        if (wasInitialized) return;
        data = newData;
        UpdateCollider();
        wasInitialized = true;
    }

    void UpdateCollider()
    {
        if (!terrainCollider) terrainCollider = GetComponent<PolygonCollider2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }

        spriteRenderer.enabled = true;
        if (!spriteRenderer.sprite) spriteRenderer.sprite = Sprite.Create(data.ChunkHeightmapTexture, new Rect(0, 0, data.TerrainGridCellSize, data.TerrainGridCellSize), new Vector2(0.5f, 0.5f), data.PixelsPerUnit);
        if (IsSolidEnough()) terrainCollider = gameObject.AddComponent<PolygonCollider2D>();
        spriteRenderer.enabled = false;
    }

    // Pixel checking on the texture to determine if the sprite has enough non-transparent pixels
    private bool IsSolidEnough(Color[] pixels = null)
    {
        if (spriteRenderer.sprite == null) return false;

        Texture2D texture = spriteRenderer.sprite.texture;
        if (pixels == null) pixels = texture.GetPixels((int)spriteRenderer.sprite.rect.x,
                                           (int)spriteRenderer.sprite.rect.y,
                                           (int)spriteRenderer.sprite.rect.width,
                                           (int)spriteRenderer.sprite.rect.height);

        int nonTransparentCount = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0)
            {
                nonTransparentCount++;
            }
        }

        float nonTransparentRatio = (float)nonTransparentCount / pixels.Length;
        return nonTransparentRatio >= minSolidChunkThreshold;
    }

    // Handling explosions by updating the sprite's texture and updating collider
    public void HandleExplosion(Vector2 explosionCenter, float explosionRadius)
    {
        if (spriteRenderer == null || terrainCollider == null) return;

        // Get the texture and pixel data
        Texture2D texture = spriteRenderer.sprite.texture;
        Rect spriteRect = spriteRenderer.sprite.rect;
        Color[] pixels = texture.GetPixels((int)spriteRect.x,
                                           (int)spriteRect.y,
                                           (int)spriteRect.width,
                                           (int)spriteRect.height);

        Vector2 spritePosition = transform.position;
        Vector2 spritePivot = new Vector2(spriteRenderer.sprite.pivot.x / spriteRect.width, spriteRenderer.sprite.pivot.y / spriteRect.height);
        Vector2 spriteScale = transform.lossyScale;

        // Iterate over each pixel and check if it's within the explosion radius
        for (int y = 0; y < spriteRect.height; y++)
        {
            for (int x = 0; x < spriteRect.width; x++)
            {
                // Calculate the pixel's world position
                Vector2 pixelWorldPos = new Vector2(
                    spritePosition.x + ((x - spritePivot.x * spriteRect.width) / data.PixelsPerUnit) * spriteScale.x,
                    spritePosition.y + ((y - spritePivot.y * spriteRect.height) / data.PixelsPerUnit) * spriteScale.y
                );

                // Check if this pixel is within the explosion's radius
                float distanceToExplosion = Vector2.Distance(pixelWorldPos, explosionCenter);

                if (distanceToExplosion <= explosionRadius)
                {
                    // Set the pixel to transparent (Color.clear)
                    pixels[x + y * (int)spriteRect.width] = Color.clear;
                }
            }
        }

        // Apply the updated pixel data back to the texture
        texture.SetPixels((int)spriteRect.x,
                          (int)spriteRect.y,
                          (int)spriteRect.width,
                          (int)spriteRect.height,
                          pixels);
        texture.Apply();

        if (!IsSolidEnough(pixels)) Destroy(gameObject);

        // Update the collider to reflect the changes
        UpdateCollider();
    }

    void DestroyCollider()
    {
        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }
    }
}
