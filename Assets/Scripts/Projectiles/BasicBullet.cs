using System.Collections;
using UnityEngine;

public class BasicBullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField, Range(0f, 100f)] float explosionStrengthRation = 4f;

    private int baseDamage;
    private float maxProjectileLifeTime = 5f;

    // Built-in methods
    // --------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectiles")) return;

        rb.velocity = Vector2.zero; // Immediately stop the bullet
        rb.position = collision.GetContact(0).point; // Move the bullet to the point of collision
        HandleCollision();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize(Vector3 direction, float shotForce, WeaponDetail weapon)
    {
        rb.AddForce(direction * shotForce);
        StartCoroutine(DestroyAfterTime());
        PlayersTurnManager.AddAwaitedBullet(gameObject);

        baseDamage = weapon.baseDamage;
        maxProjectileLifeTime = weapon.maxProjectileLifeTime;
        rb.mass *= 1 + weapon.projectileMass / 10f;
    }

    void HandleCollision()
    {
        ExplosionManager.Instance.CreateExplosion(explosionStrengthRation, transform.position, baseDamage);
        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(maxProjectileLifeTime);
        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyObject()
    {
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        PlayersTurnManager.RemoveAwaitedBullet(gameObject);

        yield return new WaitForSeconds(1.2f); // Wait for trail effect to finish
        Destroy(gameObject);
    }
}
