using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
public class TerrainGridSegment : MonoBehaviour
{
    [SerializeField] private float minPixelThreshold = 0.005f;
    private PolygonCollider2D terrainCollider;
    private SpriteRenderer spriteRenderer;

    // Filled upon creation
    public float pixelsPerUnit = 100f;
    public int terrainGridCellSize = 32;

    void Start()
    {
        UpdateCollider();
    }

    void OnDestroy()
    {
        DestroyCollider();
    }

    public void UpdateCollider(Sprite newSprite = null)
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }

        spriteRenderer.enabled = true;
        if (newSprite != null) spriteRenderer.sprite = newSprite;

        if (HasEnoughNonTransparentPixels())
        {
            terrainCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        spriteRenderer.enabled = false;
    }

    private bool HasEnoughNonTransparentPixels(Color[] pixels = null)
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
        return nonTransparentRatio >= minPixelThreshold;
    }

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
                    spritePosition.x + ((x - spritePivot.x * spriteRect.width) / pixelsPerUnit) * spriteScale.x,
                    spritePosition.y + ((y - spritePivot.y * spriteRect.height) / pixelsPerUnit) * spriteScale.y
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

        if (!HasEnoughNonTransparentPixels(pixels)) Destroy(gameObject);

        // Update the collider to reflect the changes
        UpdateCollider();
    }

    public void DestroyCollider()
    {
        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }
    }

    public void DisableCollider()
    {
        if (terrainCollider != null)
        {
            terrainCollider.enabled = false;
            DestroyCollider();
        }
    }
}
