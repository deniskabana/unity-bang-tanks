using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicBullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField, Range(0f, 100f)] float explosionStrengthRation = 4f;

    private float baseDamage = 1f;
    private float maxProjectileLifeTime = 5f;

    // Built-in methods
    // --------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
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

        baseDamage = weapon.baseDamage;
        maxProjectileLifeTime = weapon.maxProjectileLifeTime;
    }

    void HandleCollision()
    {
        ExplosionManager.Instance.CreateExplosion(explosionStrengthRation, transform.position);
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

        yield return new WaitForSeconds(1.2f); // Wait for trail effect to finish
        Destroy(gameObject);
    }
}
