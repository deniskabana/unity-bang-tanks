using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
public class TerrainGridSegment : MonoBehaviour
{
    [SerializeField] private float minPixelThreshold = 0.005f;
    private PolygonCollider2D terrainCollider;
    private SpriteRenderer spriteRenderer;

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

    private bool HasEnoughNonTransparentPixels()
    {
        if (spriteRenderer.sprite == null) return false;

        Texture2D texture = spriteRenderer.sprite.texture;
        Color[] pixels = texture.GetPixels((int)spriteRenderer.sprite.rect.x,
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
