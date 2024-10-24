using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[@RequireComponent(typeof(SpriteRenderer))]
[@RequireComponent(typeof(TerrainGridSegment))]
public class SegmentDestructibleTerrain : MonoBehaviour
{
    private List<int> handledExplosionIds = new List<int>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Explosion"))
        {
            if (handledExplosionIds.Contains(other.GetInstanceID())) return;

            CircleCollider2D explosionCollider = other.GetComponent<CircleCollider2D>();
            if (explosionCollider != null)
            {
                Vector2 explosionCenter = explosionCollider.bounds.center;
                float explosionRadius = explosionCollider.radius;

                // Get the bounds of the SpriteRenderer
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                Bounds bounds = spriteRenderer.bounds;

                // Get the corners of the bounds
                Vector2[] corners = new Vector2[4];
                corners[0] = new Vector2(bounds.min.x, bounds.min.y);
                corners[1] = new Vector2(bounds.max.x, bounds.min.y);
                corners[2] = new Vector2(bounds.min.x, bounds.max.y);
                corners[3] = new Vector2(bounds.max.x, bounds.max.y);

                bool allCornersInside = true;
                foreach (Vector2 corner in corners)
                {
                    if (Vector2.Distance(corner, explosionCenter) > explosionRadius)
                    {
                        allCornersInside = false;
                        break;
                    }
                }

                if (allCornersInside)
                {
                    Destroy(gameObject);
                }

                TerrainGridSegment terrainGridSegment = GetComponent<TerrainGridSegment>();
                terrainGridSegment.HandleExplosion(explosionCenter, explosionRadius);
                handledExplosionIds.Add(other.GetInstanceID());
            }
        }
    }
}
