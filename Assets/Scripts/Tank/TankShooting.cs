using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting settings")]
    [SerializeField] float cannonMinMaxAngle = 90f;

    [Header("References")]
    [SerializeField] Transform cannonTransform;  // Reference to the cannon's transform

    // Runtime variables
    // --------------------------------------------------

    private float shotForce;  // Force to apply to the bullet
    private float shotAngle;  // Force to apply to the bullet

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        shotAngle = cannonMinMaxAngle;
        shotForce = 0f;
    }

    // Custom methods
    // --------------------------------------------------

    public void AimCannonInstantly(float angle)
    {
        shotAngle = Mathf.Clamp(angle, -cannonMinMaxAngle, cannonMinMaxAngle);
        cannonTransform.localRotation = Quaternion.Euler(0, 0, shotAngle);
    }

    public void AimCannon(float angleSpeed)
    {
        shotAngle = Mathf.Clamp(shotAngle + angleSpeed * Time.deltaTime, -cannonMinMaxAngle, cannonMinMaxAngle);
        cannonTransform.localRotation = Quaternion.Euler(0, 0, shotAngle);
    }

    public void Shoot(float force)
    {
        shotForce = force; // This should be retrieved from the UI
        // Get the current bullet prefab, instantiate, apply forces
    }
}
