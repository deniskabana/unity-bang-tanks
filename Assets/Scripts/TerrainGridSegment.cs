using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
public class TerrainGridSegment : MonoBehaviour
{
    private PolygonCollider2D terrainCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        UpdateCollider();
    }

    void OnDestroy()
    {
        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }
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
        terrainCollider = gameObject.AddComponent<PolygonCollider2D>();
        // Create a new PolygonCollider2D based on alpha cutoff (use the alpha channel in the texture to avoid transparency artifacts)
        terrainCollider.autoTiling = false;
        spriteRenderer.enabled = false;
    }
}
