using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f;  // Tank's horizontal speed
    [SerializeField] private float groundRaycastDistance = 0.15f;  // Small distance for ground detection
    [SerializeField] private float slopeRaycastDistance = 0.25f;  // Distance to check for ground ahead
    [SerializeField] private float maxSlopeAngle = 60f;  // Maximum distance between jumps
    [SerializeField] private LayerMask groundLayer;  // Layer to detect ground

    [Header("Physics Settings")]
    [SerializeField] private float gravityForce = 9.8f;  // Custom gravity force
    [SerializeField] private float downhillSpeedMultiplier = 1.5f; // Speed multiplier when going downhill


    [Header("References")]
    [SerializeField] private Transform groundCheck;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] private Transform raycastOrigin;  // Empty GameObject at the bottom of the tank for ground detection
    [SerializeField] private BoxCollider2D boxCollider;  // The tank's BoxCollider2D

    // Private variables
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
        HandleMovement();
        CheckGround();
        ApplyGravity();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheck.position, raycastOrigin.position + Vector3.down * groundRaycastDistance);
        Gizmos.DrawWireSphere(groundCheck.position, 0.05f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(raycastOrigin.position, slopeRaycastDistance);
    }

    // Custom methods
    // --------------------------------------------------

    private void HandleMovement()
    {
        // Read horizontal input (A/D or arrow keys)
        float inputX = Input.GetAxisRaw("Horizontal");
        isMoving = inputX != 0;
        if (!isMoving || !isGrounded) return;

        // Cast a ray downward to find the ground
        RaycastHit2D groundHit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundRaycastDistance * 100f, groundLayer);
        if (groundHit.collider == null) return;

        // Cast a ray forward to detect the slope ahead
        Vector2 forwardDirection = new Vector2(inputX, 0).normalized;
        RaycastHit2D slopeHit = Physics2D.Raycast(raycastOrigin.position, forwardDirection, slopeRaycastDistance, groundLayer);

        float currentSpeed = speed;

        if (slopeHit.collider != null)
        {
            // Calculate the actual slope angle considering movement direction
            Vector2 slopeNormal = slopeHit.normal;
            float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);

            // Determine if we're going uphill or downhill
            float slopeDirection = Vector2.Dot(forwardDirection, Vector2.right);
            float normalDirection = Vector2.Dot(slopeNormal, Vector2.right);

            bool isGoingUphill = (slopeDirection > 0 && normalDirection < 0) || (slopeDirection < 0 && normalDirection > 0);

            if (isGoingUphill)
            {
                // Check if slope is too steep
                if (slopeAngle > maxSlopeAngle) return;

                // Optional: Reduce speed when going uphill
                currentSpeed *= Mathf.Lerp(1f, 0.5f, slopeAngle / maxSlopeAngle);
            }
            else
            {
                // Increase speed when going downhill
                currentSpeed *= downhillSpeedMultiplier;
            }
        }

        // Update Y position to follow ground
        float y = groundHit.point.y + (raycastOrigin.position.y - groundCheck.position.y);
        transform.position = new Vector2(transform.position.x, y);

        // Apply movement with adjusted speed
        transform.Translate(inputX * currentSpeed * Time.deltaTime * Vector3.right);
    }

    private void ApplyGravity()
    {
        if (!isGrounded) // Apply custom gravity when not grounded
        {
            velocity.y -= gravityForce * Time.deltaTime;
            transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);
        }
    }

    private void CounterGravity()
    {
        velocity.y = 0;
    }

    private void CheckGround()
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

    private void ResetGroundPosition()
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
