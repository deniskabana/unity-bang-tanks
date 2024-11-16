using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting settings")]
    [SerializeField] float cannonMinMaxAngle = 90f;
    [SerializeField] float minBulletForce = 5f;
    [SerializeField] float maxBulletForce = 50f;
    [SerializeField] float cannonAimSpeed = 40f;

    [Header("References")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform cannonTransform;  // Reference to the cannon's transform
    [SerializeField] Transform firingPoint;  // Reference to the bullet spawn point

    // Runtime variables
    // --------------------------------------------------

    private float shotForce = 20;  // Force to apply to the bullet
    private float shotAngle = 0;  // Force to apply to the bullet

    // Custom methods
    // --------------------------------------------------

    void Start()
    {
        // For preview purposes
        shotAngle = 30f;
        AimCannonInstantly(shotAngle);
    }

    public void Initialize()
    {
        shotAngle = Random.Range(-cannonMinMaxAngle / 2, cannonMinMaxAngle / 2);
        AimCannonInstantly(shotAngle);
        shotForce = 0f;
    }

    public void AimCannonInstantly(float angle)
    {
        shotAngle = Mathf.Clamp(angle, -cannonMinMaxAngle, cannonMinMaxAngle);
        cannonTransform.localRotation = Quaternion.Euler(0, 0, shotAngle + 90);
    }

    public void HandleAiming(float angleSpeed)
    {
        shotAngle = Mathf.Clamp(shotAngle + angleSpeed * cannonAimSpeed * Time.deltaTime, -cannonMinMaxAngle, cannonMinMaxAngle);
        cannonTransform.localRotation = Quaternion.Euler(0, 0, shotAngle + 90);
    }

    public GameObject Shoot(float force)
    {
        shotForce = force; // This should be retrieved from the UI
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
        Vector3 direction = firingPoint.position - cannonTransform.position;
        bullet.GetComponent<BasicBullet>().Initialize(direction, shotForce);
        return bullet;
    }
}
