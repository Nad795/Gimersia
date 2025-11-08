using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Collider2D playerCollider;

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
    [SerializeField] private float moveSpeed = 10f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float variableJumpMultiplier = 0.5f;

    [Header("Wall Bounce")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.7f;
    // [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallBounceForce = new Vector2(8f, 13f);
    [SerializeField] private float wallBounceLockoutTime = 0.3f;
    [SerializeField] private float wallBounceGraceTime = 0.1f;
    // private bool isWallSliding;
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
    }

    private void Update()
    {
        horizontalInput = moveInput.x;

        HandleChecks();
        Flip();


        // Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }


        // Jump Buffering
        if (jumpPressed)
        {
            jumpBufferTimeCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferTimeCounter -= Time.deltaTime;
        }

        // Wall Jump Lockout
        if (wallBounceLockoutTimeCounter > 0f)
        {
            wallBounceLockoutTimeCounter -= Time.deltaTime;
        }


        // Wall Bounce Grace Time
        bool justTouchedWall = isTouchingWall && !wasTouchingWallLastFrame && !isGrounded;
        wasTouchingWallLastFrame = isTouchingWall;

        if (justTouchedWall)
        {
            wallBounceGraceTimeCounter = wallBounceGraceTime;
        }
        else
        {
            wallBounceGraceTimeCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (wallBounceLockoutTimeCounter <= 0f && !justWallBounced)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        bool canGroundJump = coyoteTimeCounter > 0f;
        bool tryBufferJump = jumpBufferTimeCounter > 0f;
        bool canWallBounce = wallBounceGraceTimeCounter > 0f;

        if (jumpPressed && canWallBounce)
        {
            rb.linearVelocity = new Vector2(-facingDirection * wallBounceForce.x, wallBounceForce.y);
            wallBounceLockoutTimeCounter = wallBounceLockoutTime;
            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            wallBounceGraceTimeCounter = 0f;
            jumpPressed = false;
            justWallBounced = true;
            facingDirection *= -1f;
        }

        else if (jumpPressed && canGroundJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            jumpPressed = false;
        }

        else if (tryBufferJump && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimeCounter = 0f;
            jumpPressed = false;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
        }

        Debug.Log(isTouchingWall);
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

    private void Flip()
    {
        if (wallBounceLockoutTimeCounter <= 0f && horizontalInput != 0 && !justWallBounced)
        {
            facingDirection = horizontalInput;
        }
        
        // Use localScale to flip the sprite (assumes sprite faces right by default)
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    public void OnMove(InputValue value)
    {

        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumpHeld = value.isPressed;

        if (jumpHeld)
        {
            jumpPressed = true;
        }
        else
        {
            jumpPressed = false;
        }
    }
}
