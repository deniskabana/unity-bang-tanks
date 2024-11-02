using UnityEngine;

public class TankPhysics : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 1f;  // Tank's horizontal speed
    [SerializeField] float groundRaycastDistance = 0.15f;  // Small distance for ground detection
    [SerializeField] float maxSlopeAngle = 70f;  // Maximum distance between jumps
    [SerializeField] LayerMask groundLayer;  // Layer to detect ground

    [Header("Physics Settings")]
    [SerializeField] float gravityForce = 9.8f;  // Custom gravity force
    [SerializeField] float downhillSpeedMultiplier = 1.5f; // Speed multiplier when going downhill


    [Header("References")]
    [SerializeField] Transform groundCheck;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] Transform raycastOrigin;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] BoxCollider2D boxCollider;  // The tank's BoxCollider2D

    // Runtime variables
    // --------------------------------------------------

    private Vector2 velocity;  // Store velocity for movement
    private bool isGrounded = false;  // Whether the tank is grounded
    private bool isMoving = false;  // Whether the tank is moving

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        ResetGroundPosition();
    }

    void Update()
    {
        CheckGround();
        ApplyGravity();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheck.position, raycastOrigin.position + Vector3.down * groundRaycastDistance);
        Gizmos.DrawWireSphere(groundCheck.position, 0.05f);
    }

    // Custom methods
    // --------------------------------------------------

    public void HandleMovement(float inputX)
    {
        isMoving = inputX != 0;
        if (!isMoving || !isGrounded) return;

        // Cast a ray downward to find the ground
        RaycastHit2D groundHit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundRaycastDistance * 100f, groundLayer);
        if (groundHit.collider == null) return;

        // Get the slope angle and movement direction
        float slopeAngle = Vector2.Angle(groundHit.normal, Vector2.up);
        bool isGoingUphill = (inputX > 0 && groundHit.normal.x < 0) || (inputX < 0 && groundHit.normal.x > 0);

        // If going uphill and slope is too steep, prevent movement
        if (isGoingUphill && slopeAngle > maxSlopeAngle) return;

        // Snap to ground and move
        float y = groundHit.point.y + (raycastOrigin.position.y - groundCheck.position.y);
        transform.position = new Vector2(transform.position.x, y);
        float speedOnSlope = isGoingUphill ? speed : speed * downhillSpeedMultiplier;
        transform.Translate(inputX * speedOnSlope * Time.deltaTime * Vector3.right);
    }

    void ApplyGravity()
    {
        if (!isGrounded) // Apply custom gravity when not grounded
        {
            velocity.y -= gravityForce * Time.deltaTime;
            transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);
        }
    }

    void CounterGravity()
    {
        velocity.y = 0;
    }

    void CheckGround()
    {
        // Raycast downward from the GroundCheck to check if the tank is grounded
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundRaycastDistance, groundLayer);

        if (groundHit.collider != null)
        {
            isGrounded = true;
            CounterGravity();
            ResetGroundPosition();
        }
        else
        {
            isGrounded = false;
        }
    }

    void ResetGroundPosition()
    {
        // Raycast downward from the GroundCheck to check if the tank is grounded
        RaycastHit2D groundHit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundRaycastDistance * 100f, groundLayer); // Super long raycast
        if (groundHit.collider != null)
        {
            float y = groundHit.point.y + (raycastOrigin.position.y - groundCheck.position.y);
            transform.position = new Vector2(transform.position.x, y);
        }
    }
}
