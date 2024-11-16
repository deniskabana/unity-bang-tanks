using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicBullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float maxLifeTime = 5f;

    // Built-in methods
    // --------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
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
        ExplosionManager.Instance.CreateExplosion(1.5f, transform.position);
        Destroy(gameObject);
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(maxLifeTime);
        Destroy(gameObject);
    }
}
