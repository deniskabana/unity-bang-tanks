using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager Instance;

    public bool debug = false;

    [Header("Explosion Visuals")]
    [SerializeField] float actionDelay = 0.15f;

    [Header("References")]
    [SerializeField] private Transform explosionParent;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject explosionParticlesPrefab;

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

    public void CreateExplosion(float radius, Vector3 position)
    {
        // Create explosion GameObject
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity, explosionParent);
        Instantiate(explosionParticlesPrefab, position, Quaternion.identity).transform.localScale = new Vector3(radius, radius, 1);

        // Set circle collider size to match the sprite size
        CircleCollider2D circleCollider = explosion.GetComponent<CircleCollider2D>();
        circleCollider.radius = radius;

        // Create permanent terrain mask
        Sprite spriteForMask = CreateCircleSprite(radius, Color.black);
        SpriteMask spriteMaskComponent = explosion.GetComponent<SpriteMask>();
        StartCoroutine(SetMaskAfterDelay(actionDelay, spriteMaskComponent, spriteForMask));
    }

    private IEnumerator SetMaskAfterDelay(float delay, SpriteMask spriteMask, Sprite mask)
    {
        yield return new WaitForSeconds(delay);
        spriteMask.sprite = mask; // Assign the explosion sprite to the SpriteMask component to uncover terrain
        gameObject.tag = "Untagged";
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
