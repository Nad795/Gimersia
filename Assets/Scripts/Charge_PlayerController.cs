using System.Collections;
using System.Dynamic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Charge_PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator anim;
    private HealthSystem healthSystem;

    private Vector2 moveInput;
    private float horizontalInput;

    private bool jumpPressed;
    private bool jumpHeld;

    // State Checks
    private bool isGrounded;
    private bool isTouchingWall;
    private bool wasTouchingWallLastFrame;
    private bool justWallBounced;
    private float facingDirection = 1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jumping")]
    [SerializeField] private Vector2 jumpForce = new Vector2(30f, 30f);
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float variableJumpMultiplier = 0.5f;
    [SerializeField] private float maxJumpHoldTime = 1f;
    private float jumpHoldTimeCounter;

    private bool isJumpCharging;


    [Header("Wall Bounce")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private Vector2 wallBounceForce = new Vector2(8f, 13f);
    [SerializeField] private float wallBounceLockoutTime = 0.3f;
    [SerializeField] private float wallBounceGraceTime = 0.1f;

    [Header("Rebound")]
    [SerializeField] private float wallReboundForce = 7f;
    [SerializeField] private float reboundVelocityDampener = 0.5f;

    private float wallDirection => horizontalInput;
    private float wallBounceLockoutTimeCounter;
    private float wallBounceGraceTimeCounter;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferTimeCounter;

    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial2D slippyMaterial;
    [SerializeField] private PhysicsMaterial2D stickyMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        horizontalInput = moveInput.x;

        HandleChecks();
        Flip();
        HandleAnimations();

        // Coyote Time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffering
        if (jumpPressed)
            jumpBufferTimeCounter = jumpBufferTime;
        else
            jumpBufferTimeCounter -= Time.deltaTime;

        // Wall Jump Lockout
        if (wallBounceLockoutTimeCounter > 0f)
            wallBounceLockoutTimeCounter -= Time.deltaTime;

        // Wall Bounce Grace
        bool justTouchedWall = isTouchingWall && !wasTouchingWallLastFrame && !isGrounded;
        wasTouchingWallLastFrame = isTouchingWall;

        if (justTouchedWall)
            wallBounceGraceTimeCounter = wallBounceGraceTime;
        else
            wallBounceGraceTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // if (wallBounceLockoutTimeCounter <= 0f && !justWallBounced)
        //     rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // bool canGroundJump = coyoteTimeCounter > 0f;
        // bool tryBufferJump = jumpBufferTimeCounter > 0f;
        // bool canWallBounce = wallBounceGraceTimeCounter > 0f;

        // if (jumpPressed && canWallBounce)
        // {
        //     rb.linearVelocity = new Vector2(-facingDirection * wallBounceForce.x, wallBounceForce.y);
        //     wallBounceLockoutTimeCounter = wallBounceLockoutTime;
        //     coyoteTimeCounter = 0f;
        //     jumpBufferTimeCounter = 0f;
        //     wallBounceGraceTimeCounter = 0f;
        //     jumpPressed = false;
        //     justWallBounced = true;
        //     facingDirection *= -1f;
        // }
        // else if (jumpPressed && canGroundJump)
        // {
        //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //     coyoteTimeCounter = 0f;
        //     jumpBufferTimeCounter = 0f;
        //     jumpPressed = false;
        // }
        // else if (tryBufferJump && isGrounded)
        // {
        //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //     jumpBufferTimeCounter = 0f;
        //     jumpPressed = false;
        // }

        // if (!jumpHeld && rb.linearVelocity.y > 0f)
        //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);

        if (isJumpCharging)
        {
            if (jumpHoldTimeCounter < maxJumpHoldTime)
            {
                jumpHoldTimeCounter += Time.fixedDeltaTime;
            }

            if (!jumpHeld)
            {
                float chargePercent = Mathf.Clamp01(jumpHoldTimeCounter / maxJumpHoldTime);
                Vector2 chargedJumpForce = new Vector2(jumpForce.x * chargePercent, jumpForce.y * chargePercent);

                rb.linearVelocity = new Vector2(facingDirection * chargedJumpForce.x, chargedJumpForce.y);

                isJumpCharging = false;
                jumpHoldTimeCounter = 0f;
                jumpPressed = false;
            }

            return;
        }
        

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

            if (jumpPressed)
            {
                isJumpCharging = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void HandleChecks()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, new Vector2(facingDirection, 0), wallCheckDistance, groundLayer);

        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckRadius, Color.green);
        Debug.DrawRay(wallCheck.position, new Vector2(facingDirection, 0) * wallCheckDistance, Color.red);

        if (isGrounded)
        {
            playerCollider.sharedMaterial = stickyMaterial;
            justWallBounced = false;
        }
        else
        {
            playerCollider.sharedMaterial = slippyMaterial;
        }
    }

    private void HandleAnimations()
    {
        // --- RUN ---
        bool isMoving = horizontalInput != 0 && isGrounded;
        anim.SetBool("isMoving", isMoving);

        // --- JUMP ---
        bool isJumping = !isGrounded && !isTouchingWall;
        anim.SetBool("isJumping", isJumping);

        // --- WALL ATTACH ---
        bool isWallAttach = isTouchingWall && !isGrounded && rb.linearVelocity.y <= 0f;
        anim.SetBool("isWallAttach", isWallAttach);
    }

    private void Flip()
    {
        if (wallBounceLockoutTimeCounter <= 0f && horizontalInput != 0 && !justWallBounced && !isJumpCharging)
            facingDirection = horizontalInput;

        transform.localScale = new Vector3(-facingDirection, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with " + collision.gameObject.name);
        // 1. We only care about collisions that happen IN THE AIR.
        if (isGrounded)
        {
            return;
        }

        // 2. We only care about collisions with the "Ground" layer.
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // 3. Get the first contact point
            ContactPoint2D contact = collision.contacts[0];

            // 4. Check if the surface normal is mostly horizontal (it's a wall)
            // A normal of (1, 0) or (-1, 0) is a perfect wall.
            // A normal of (0, 1) is a floor.
            if (Mathf.Abs(contact.normal.x) > 0.7f)
            {
                // --- THIS IS THE BONK ---

                // Apply the rebound:
                // - We use 'contact.normal.x' to push us AWAY from the wall
                // - We dampen our current vertical velocity to make us fall
                rb.linearVelocity = new Vector2(
                    contact.normal.x * wallReboundForce, 
                    rb.linearVelocity.y * reboundVelocityDampener
                );

                // Also, make the player face away from the wall
                // The normal's X will be 1 or -1, so we can use it to set direction
                facingDirection = Mathf.Sign(contact.normal.x);
                transform.localScale = new Vector3(-facingDirection, 1, 1);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumpHeld = value.isPressed;

        if (jumpHeld)
            jumpPressed = true;
        else
            jumpPressed = false;
    }

    public void Die()
    {
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }

    // Called by Animation Event at the end of Die animation
    public void OnDeathAnimationEnd()
    {
        // Reload the current scene
        healthSystem.ActivateGameOverPanel();
    }
}
