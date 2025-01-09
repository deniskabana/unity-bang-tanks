using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting settings")]
    [SerializeField] float cannonMinMaxAngle = 90f;
    [SerializeField] float cannonAimSpeed = 40f;

    [Header("References")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform cannonTransform;  // Reference to the cannon's transform
    [SerializeField] Transform firingPoint;  // Reference to the bullet spawn point

    // Runtime variables
    // --------------------------------------------------

    private float shotForce = 20;  // Force to apply to the bullet
    private float shotAngle = 0;  // Force to apply to the bullet
    private int weaponIndex = 0;  // Index of the current weapon

    private List<WeaponDetail> weapons;

    // Custom methods
    // --------------------------------------------------

    void Start()
    {
        // For preview purposes
        shotAngle = 30f;
        AimCannonInstantly(shotAngle);
        weapons = WeaponSelectionManager.GetWeaponsData();
    }

    public void Initialize()
    {
        shotAngle = Random.Range(-cannonMinMaxAngle / 2, cannonMinMaxAngle / 2);
        AimCannonInstantly(shotAngle);
        weapons = WeaponSelectionManager.GetWeaponsData();
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
        WeaponDetail currentWeapon = GetCurrentWeapon();
        shotForce = force; // This is retrieved from UI

        // TODO: ProjectileType.Grenade and others support

        // ProjectileType.Bullet
        if (currentWeapon.projectileSpawnCount == 1)
        {
            GameObject bullet = Instantiate(currentWeapon.projectilePrefab, firingPoint.position, firingPoint.rotation);
            Vector3 direction = firingPoint.position - cannonTransform.position;
            bullet.GetComponent<BasicBullet>().Initialize(direction, shotForce, currentWeapon);
            return bullet;
        }
        else
        {
            // ProjectileType.Spread
            List<GameObject> bullets = new List<GameObject>();
            for (int i = 0; i < currentWeapon.projectileSpawnCount; i++)
            {
                float projectileSpread = 10f;

                GameObject bullet = Instantiate(currentWeapon.projectilePrefab, firingPoint.position, firingPoint.rotation);
                Vector3 direction = firingPoint.position - cannonTransform.position;
                // Spread the bullets
                direction = Quaternion.Euler(0, 0, Random.Range(-projectileSpread, projectileSpread)) * direction;
                bullet.GetComponent<BasicBullet>().Initialize(direction, shotForce, currentWeapon);
                bullets.Add(bullet);
            }
            return bullets[0];
        }
    }

    public void SetNextWeapon()
    {
        weaponIndex = (weaponIndex + 1) % weapons.Count;
    }

    public WeaponDetail GetCurrentWeapon()
    {
        return weapons[weaponIndex];
    }
}
