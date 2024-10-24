using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;  // Tank's horizontal speed
    [SerializeField] private float maxClimbAngle = 30f;  // Maximum slope angle tank can climb
    [SerializeField] private float groundRaycastDistance = 0.1f;  // Small distance for ground detection
    [SerializeField] private float slopeRaycastDistance = 1.5f;  // Distance to check for ground ahead
    [SerializeField] private LayerMask groundLayer;  // Layer to detect ground

    [Header("Physics Settings")]
    [SerializeField] private float gravityForce = 9.8f;  // Custom gravity force

    [Header("References")]
    [SerializeField] private Transform groundCheck;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] private BoxCollider2D boxCollider;  // The tank's BoxCollider2D

    // Private variables
    // --------------------------------------------------

    private Vector2 velocity;  // Store velocity for movement
    private bool isGrounded = true;  // Whether the tank is grounded
    private bool isFalling = false;  // Whether the tank is falling


    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
    }

    void Update()
    {
        if (isGrounded && !isFalling)
        {
            HandleMovement();
        }
        ApplyGravity();
        CheckGround();
    }

    // private void OnDrawGizmos()
    // {
    //     // Ground check raycast (small distance for precise ground detection)
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundRaycastDistance);

    //     // Slope raycast to detect the terrain ahead of the tank
    //     float inputX = Input.GetAxisRaw("Horizontal");
    //     if (inputX != 0)
    //     {
    //         Gizmos.color = Color.red;
    //         float rayOriginX = inputX > 0 ? boxCollider.bounds.max.x : boxCollider.bounds.min.x;
    //         Vector2 rayOrigin = new Vector2(rayOriginX, transform.position.y);
    //         Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * slopeRaycastDistance);
    //     }
    // }

    // Custom methods
    // --------------------------------------------------

    private void HandleMovement()
    {
        // Read horizontal input (A/D or arrow keys)
        float inputX = Input.GetAxisRaw("Horizontal");

        if (inputX != 0)
        {
            // Get the tank's forward edge based on the collider size
            float rayOriginX = inputX > 0 ? boxCollider.bounds.max.x : boxCollider.bounds.min.x;
            Vector2 rayOrigin = new Vector2(rayOriginX, transform.position.y);

            // Raycast ahead in the direction of movement to detect the slope or ground
            RaycastHit2D slopeHit = Physics2D.Raycast(rayOrigin, Vector2.down, boxCollider.bounds.extents.y + slopeRaycastDistance, groundLayer);

            if (slopeHit.collider != null)
            {
                // Calculate the slope angle based on the surface normal
                float slopeAngle = Vector2.Angle(slopeHit.normal, Vector2.up);

                if (slopeAngle <= maxClimbAngle)
                {
                    // Set the new position based on raycast hit Y position (terrain following)
                    float groundY = slopeHit.point.y;
                    transform.position = new Vector2(transform.position.x + inputX * speed * Time.deltaTime, groundY);
                }
                else
                {
                    // Stop horizontal movement if slope is too steep
                    velocity.x = 0;
                }
            }
            else
            {
                // If no ground detected ahead, set tank to falling
                isGrounded = false;
            }
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y -= gravityForce * Time.deltaTime;
            transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);
        }
    }

    private void CheckGround()
    {
        // Raycast downward from the GroundCheck to check if the tank is grounded
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundRaycastDistance, groundLayer);

        if (groundHit.collider != null)
        {
            isGrounded = true;
            isFalling = false;
            velocity.y = 0;  // Reset vertical velocity when grounded
        }
        else
        {
            isGrounded = false;
            isFalling = true;
        }
    }
}
