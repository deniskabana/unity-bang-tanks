using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        ControlsManager.OnControlUp.AddListener(OnNextWeaponControlButton);
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
        WeaponDetail currentWeapon = GetCurrentWeapon();
        shotForce = force; // This is retrieved from UI
        GameObject bullet = Instantiate(currentWeapon.projectilePrefab, firingPoint.position, firingPoint.rotation);
        Vector3 direction = firingPoint.position - cannonTransform.position;

        switch (currentWeapon.type)
        {
            case ProjectileType.Cannonball:
                bullet.GetComponent<BasicBullet>().Initialize(direction, shotForce, currentWeapon);
                break;

            case ProjectileType.Grenade:
                // bullet.GetComponent<Grenade>().Initialize(direction, shotForce);
                Debug.LogError("Grenade not implemented yet");
                break;
        }

        return bullet;
    }

    public void SetNextWeapon()
    {
        weaponIndex = (weaponIndex + 1) % weapons.Count;
        UIManager.SetActiveWeapon(weapons[weaponIndex]);
    }

    public WeaponDetail GetCurrentWeapon()
    {
        return weapons[weaponIndex];
    }

    private void OnNextWeaponControlButton(ControlType controlType)
    {
        if (controlType == ControlType.WeaponNext)
        {
            SetNextWeapon();
        }
    }
}
