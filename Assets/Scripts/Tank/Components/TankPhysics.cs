using UnityEngine;

public class TankPhysics : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 0.2f;  // Tank's horizontal speed
    [SerializeField] float groundRaycastDistance = 0.15f;  // Small distance for ground detection
    [SerializeField] float maxSlopeAngle = 70f;  // Maximum distance between jumps
    [SerializeField] LayerMask groundLayer;  // Layer to detect ground

    [Header("Physics Settings")]
    [SerializeField] float gravityForce = 9.8f;  // Custom gravity force
    [SerializeField] float downhillSpeedMultiplier = 1.4f; // Speed multiplier when going downhill

    [Header("References")]
    [SerializeField] Transform groundCheck;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] Transform raycastOrigin;  // Empty GameObject at the bottom of the tank for ground detection

    // Runtime variables
    // --------------------------------------------------

    private Vector2 velocity;  // Store velocity for movement
    public bool isGrounded = false;  // Whether the tank is grounded
    private bool isMoving = false;  // Whether the tank is moving
    private bool initialized = false;

    // Built-in methods
    // --------------------------------------------------

    public void Initialize()
    {
        initialized = true;
        ResetGroundPosition();
    }

    void Update()
    {
        if (!initialized) return;
        CheckGround();
        ApplyGravity();
    }

    void OnDrawGizmos()
    {
        if (!initialized) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheck.position, raycastOrigin.position + Vector3.down * groundRaycastDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(raycastOrigin.position, groundRaycastDistance);
    }

    // Custom methods
    // --------------------------------------------------

    public float HandleMovement(float inputX)
    {
        isMoving = inputX != 0;
        if (!isMoving || !isGrounded) return 0;

        // Cast a ray downward to find the ground
        RaycastHit2D groundHit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundRaycastDistance * 100f, groundLayer);
        if (groundHit.collider == null) return 0;

        // Get the slope angle and movement direction
        float slopeAngle = Vector2.Angle(groundHit.normal, Vector2.up);
        bool isGoingUphill = (inputX > 0 && groundHit.normal.x < 0) || (inputX < 0 && groundHit.normal.x > 0);

        // If going uphill and slope is too steep, prevent movement
        if (isGoingUphill && slopeAngle > maxSlopeAngle) return 0;

        // Snap to ground and move
        float y = groundHit.point.y + (raycastOrigin.position.y - groundCheck.position.y);
        transform.position = new Vector2(transform.position.x, y);
        float speedOnSlope = isGoingUphill ? speed : speed * downhillSpeedMultiplier;
        float finalAppliedSpeed = speedOnSlope * Time.deltaTime * inputX;
        transform.Translate(finalAppliedSpeed * Vector3.right);

        return finalAppliedSpeed;
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
