using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    [Header("Explosion Visuals")]
    [SerializeField] float maxSpriteLifetime = 0.25f;

    [Header("References")]
    [SerializeField] private GameObject explosionPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 since we are in 2D
            float radius = Random.Range(0.5f, 1.5f);
            CreateExplosion(radius, mousePosition);
        }
    }

    public void CreateExplosion(float radius, Vector3 position)
    {
        // Create explosion GameObject
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        // TODO: replace with animation / sprite / whatever
        Sprite circleSprite = CreateCircleSprite(radius, Color.yellow);

        // Add a SpriteRenderer component to the explosion and assign sprite
        SpriteRenderer spriteRenderer = explosion.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = circleSprite;
        spriteRenderer.sortingOrder = 1; // Set sorting order to render above terrain and player

        // Set circle collider size to match the sprite size
        CircleCollider2D circleCollider = explosion.GetComponent<CircleCollider2D>();
        circleCollider.radius = radius;

        // Clear spriteRenderer.sprite after maxSpriteLifetime
        StartCoroutine(ClearSpriteAfterDelay(spriteRenderer, maxSpriteLifetime)); // Clear sprite after a delay

        // Create permanent terrain mask
        Sprite spriteForMask = CreateCircleSprite(radius, Color.black);
        SpriteMask spriteMaskComponent = explosion.GetComponent<SpriteMask>();
        spriteMaskComponent.sprite = spriteForMask; // Assign the explosion sprite to the SpriteMask component to uncover terrain
    }

    private IEnumerator ClearSpriteAfterDelay(SpriteRenderer spriteRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        spriteRenderer.sprite = null;
        Destroy(spriteRenderer);
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
