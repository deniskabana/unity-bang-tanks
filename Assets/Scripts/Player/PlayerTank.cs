using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1.25f;  // Tank's horizontal speed
    [SerializeField] private float groundRaycastDistance = 0.15f;  // Small distance for ground detection
    [SerializeField] private float slopeRaycastDistance = 0.25f;  // Distance to check for ground ahead
    [SerializeField] private float maxSlopeAngle = 60f;  // Maximum distance between jumps
    [SerializeField] private LayerMask groundLayer;  // Layer to detect ground

    [Header("Physics Settings")]
    [SerializeField] private float gravityForce = 9.8f;  // Custom gravity force

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

    // Custom methods
    // --------------------------------------------------

    private void HandleMovement()
    {
        // Read horizontal input (A/D or arrow keys)
        float inputX = Input.GetAxisRaw("Horizontal");
        isMoving = inputX != 0;
        if (!isMoving) return;
        if (!isGrounded) return; // if the tank is mid-air, don't move it

        RaycastHit2D groundHit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundRaycastDistance * 100f, groundLayer); // Super long raycast

        if (groundHit.collider != null)
        {
            float y = groundHit.point.y + (raycastOrigin.position.y - groundCheck.position.y);
            float slopeAngle = Vector2.Angle(groundHit.normal, Vector2.up);

            // TODO: Fix the slope angle calculation - it stops tank movement even if there are downward slopes, not just upward slopes
            Debug.Log(slopeAngle);

            if (slopeAngle > maxSlopeAngle) return;
            // Snap and fix Y position to the ground
            transform.position = new Vector2(transform.position.x, y);
        }

        // Apply horziontal movement
        transform.Translate(inputX * speed * Time.deltaTime * Vector3.right);
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
