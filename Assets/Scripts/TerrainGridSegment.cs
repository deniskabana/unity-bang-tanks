using UnityEngine;

[@RequireComponent(typeof(SpriteMask))]
public class TerrainGridSegment : MonoBehaviour
{
    private PolygonCollider2D terrainCollider;

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

    public void UpdateCollider()
    {
        if (terrainCollider != null)
        {
            Destroy(terrainCollider);
            terrainCollider = null;
        }

        // Set sprite temporarily to make sure the collider is the same shape as the sprite
        SpriteRenderer tempSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (tempSpriteRenderer == null) tempSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        tempSpriteRenderer.sprite = GetComponent<SpriteMask>().sprite;

        terrainCollider = gameObject.AddComponent<PolygonCollider2D>();
        Destroy(tempSpriteRenderer);
    }
}
