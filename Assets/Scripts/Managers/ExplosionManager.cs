using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager Instance;

    public bool debug = false;

    [Header("Explosion")]
    [SerializeField] public float maxExplosionDamage = 20f;
    [SerializeField] float visualActionDelay = 0.15f;

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

    public void CreateExplosion(float radius, Vector3 position, float damage)
    {
        // Create explosion GameObject
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity, explosionParent);
        Instantiate(explosionParticlesPrefab, position, Quaternion.identity).transform.localScale = new Vector3(radius * 0.85f, radius * 0.85f, 1);

        explosion.GetComponent<ExplosionDamage>().explosionDamage = damage;

        // Set circle collider size to match the sprite size
        CircleCollider2D circleCollider = explosion.GetComponent<CircleCollider2D>();
        circleCollider.radius = radius;

        // Create permanent terrain mask
        Sprite spriteForMask = CreateCircleSprite(radius, Color.black);
        SpriteMask spriteMaskComponent = explosion.GetComponent<SpriteMask>();
        StartCoroutine(WaitSetMaskDisableCollider(visualActionDelay, spriteMaskComponent, spriteForMask));
    }

    private IEnumerator WaitSetMaskDisableCollider(float delay, SpriteMask spriteMask, Sprite mask)
    {
        yield return new WaitForSeconds(delay);
        spriteMask.sprite = mask; // Assign the explosion sprite to the SpriteMask component to uncover terrain
        spriteMask.gameObject.GetComponent<CircleCollider2D>().enabled = false; // Disable the collider to hits after explosion
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
