using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    // Serialized fields
    // --------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float slopeCheckDistance = 0.5f;
    [SerializeField] private float groundCheckRadius = 0.4f;

    [Header("References")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D fullFriction;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Transform groundCheck;

    // Private fields
    // --------------------------------------------------

    private Vector2 newVelocity;
    private Vector2 colliderSize;
    private Vector2 slopeNormalPerp;

    private float moveInput;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private float slopeSideAngle;

    private bool isOnSlope = false;
    private bool isGrounded = false;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        colliderSize = capsuleCollider.size;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rigidBody.velocity = new Vector2(moveInput * speed, rigidBody.velocity.y);
    }

    void FixedUpdate()
    {
        CheckGround();
        SlopeCheck();
        ApplyMovement();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // Custom implementations
    // --------------------------------------------------

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckRadius, whatIsGround);
    }

    private void SlopeCheck()
    {
        Vector2 checkPosition = transform.position - new Vector3(0.0f, capsuleCollider.offset.y + colliderSize.y / 2);
        SlopeCheckVertical(checkPosition);
        SlopeCheckHorizontal(checkPosition);
    }

    private void ApplyMovement()
    {
        // Apply movement for grounded and airborne states
        newVelocity.Set(speed * moveInput, rigidBody.velocity.y);
        rigidBody.velocity = newVelocity;

        if (isGrounded && !isOnSlope)
        {
            newVelocity.Set(speed * moveInput, 0.0f);
        }
        else if (isGrounded && isOnSlope)
        {
            newVelocity.Set(speed * slopeNormalPerp.x * -moveInput, speed * slopeNormalPerp.y * -moveInput);
        }
        else if (!isGrounded)
        {
            newVelocity.Set(speed * moveInput, rigidBody.velocity.y);
        }

        rigidBody.velocity = newVelocity;

        // Apply friction based on movement
        if (moveInput == 0)
        {
            capsuleCollider.sharedMaterial = fullFriction;
        }
        else
        {
            capsuleCollider.sharedMaterial = noFriction;
        }

    }

    private void SlopeCheckHorizontal(Vector2 checkPosition)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPosition, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPosition, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, slopeCheckDistance, whatIsGround);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != slopeDownAngleOld)
            {
                isOnSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
    }
}
