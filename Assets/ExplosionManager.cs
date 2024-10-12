using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject spriteMaskPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 since we are in 2D
            float radius = Random.Range(0.5f, 1.5f);
            CreateExplosionVisual(radius, mousePosition);
            CreateExplosionMask(radius, mousePosition);
        }
    }

    public void CreateExplosionMask(float radius, Vector3 position)
    {
        GameObject spriteMask = Instantiate(spriteMaskPrefab, position, Quaternion.identity);
        spriteMask.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
    }

    public void CreateExplosionVisual(float radius, Vector3 position)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        // Add a SpriteRenderer component to the explosion
        SpriteRenderer spriteRenderer = explosion.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite(radius, Color.yellow); // Create and assign a yellow circle sprite
        spriteRenderer.sortingOrder = 1; // Set sorting order to render above terrain and player
        Destroy(explosion, 0.25f); // Destroy the explosion after 0.25 seconds
    }

    Sprite CreateCircleSprite(float radius, Color color)
    {
        int textureSize = Mathf.CeilToInt(radius * 2 * 100); // 100 pixels per unit
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] colors = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(textureSize / 2, textureSize / 2));
                if (distance <= radius * 100)
                {
                    colors[y * textureSize + x] = color;
                }
                else
                {
                    colors[y * textureSize + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100);
    }
}
