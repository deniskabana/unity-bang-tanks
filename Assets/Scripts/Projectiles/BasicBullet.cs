using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicBullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    public void Initialize(Vector3 direction, float shotForce)
    {
        rb.AddForce(direction * shotForce);
    }

    void HandleCollision()
    {
        ExplosionManager.Instance.CreateExplosion(1.5f, transform.position);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision();
    }
}
