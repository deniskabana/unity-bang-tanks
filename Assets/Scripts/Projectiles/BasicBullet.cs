using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicBullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float maxLifeTime = 13f;

    // Built-in methods
    // --------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector2.zero; // Immediately stop the bullet
        HandleCollision();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize(Vector3 direction, float shotForce)
    {
        rb.AddForce(direction * shotForce);
        StartCoroutine(DestroyAfterTime());
    }

    void HandleCollision()
    {
        ExplosionManager.Instance.CreateExplosion(0.75f, transform.position);
        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(maxLifeTime);
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
