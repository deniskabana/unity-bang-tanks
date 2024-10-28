using Unity.VisualScripting;
using UnityEngine;

public class TankHealth : MonoBehaviour
{
  [Header("Health Settings")]
  [SerializeField] int maxHealth = 100;

  [Header("Render Settings")]
  [SerializeField] Color backgroundColor = Color.red;
  [SerializeField] Color foregroundColor = Color.green;
  [SerializeField] int width = 64;
  [SerializeField] int height = 8;

  [Header("References")]
  [SerializeField] GameObject healthbarPrefab;

  private bool initialized = false;
  private int currentHealth;
  private GameObject healthbarObject;
  private SpriteRenderer foregroundSpriteRenderer;
  private SpriteRenderer backgroundSpriteRenderer;

  void Start()
  {
    currentHealth = maxHealth;
    Initialize();
  }

  void Initialize()
  {
    if (initialized) return;

    healthbarObject = Instantiate(healthbarPrefab, transform.position, Quaternion.identity, gameObject.transform);
    foregroundSpriteRenderer = healthbarObject.AddComponent<SpriteRenderer>();
    backgroundSpriteRenderer = healthbarObject.AddComponent<SpriteRenderer>();

    foregroundSpriteRenderer.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    foregroundSpriteRenderer.color = foregroundColor;

    DrawHealthBar();
    initialized = false;
  }

  public void TakeDamage(int damage)
  {
    maxHealth -= damage;
    if (maxHealth <= 0)
    {
      Die();
    }

    DrawHealthBar();
  }

  public void DrawHealthBar()
  {
    if (backgroundSpriteRenderer.sprite == null)
      backgroundSpriteRenderer.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

    if (foregroundSpriteRenderer.sprite == null)
      foregroundSpriteRenderer.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

    backgroundSpriteRenderer.color = backgroundColor;
    foregroundSpriteRenderer.color = foregroundColor;

    backgroundSpriteRenderer.size = new Vector2(width, height);
    foregroundSpriteRenderer.size = new Vector2(width * (currentHealth / maxHealth), height);
  }

  void Die()
  {
    Destroy(gameObject);
  }
}