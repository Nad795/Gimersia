using System.Collections;
using System.Dynamic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator anim;
    private HealthSystem healthSystem;

    private SpriteRenderer playerSprite;

    private Vector2 moveInput;
    private float horizontalInput;

    private bool jumpPressed;
    private bool jumpHeld;

    // State Checks
    private bool isGrounded;
    private bool isTouchingWall;
    // private bool wasTouchingWallLastFrame;
    private bool justWallBounced;
    private float facingDirection = 1f;
    private float originalGravityScale;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float variableJumpMultiplier = 0.5f;

    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 1f;
    private bool dashPressed;
    private bool canDash;
    private bool isDashing;
    private float dashDurationCounter;
    private float dashCooldownCounter;

    [Header("Wall Bounce")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 1f);
    [SerializeField] private Vector2 wallBounceForce = new Vector2(8f, 13f);
    [SerializeField] private float wallBounceLockoutTime = 0.3f;
    // [SerializeField] private float wallBounceGraceTime = 0.1f;

    private float wallDirection => horizontalInput;
    private float wallBounceLockoutTimeCounter;
    // private float wallBounceGraceTimeCounter;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferTimeCounter;

    [Header("Ghost Effect")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float ghostSpawnRate = 0.04f; // How often to spawn a ghost
    private float ghostSpawnTimer;

    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial2D slippyMaterial;
    [SerializeField] private PhysicsMaterial2D stickyMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        playerSprite = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale;
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

        // Dash Cooldown
        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;
        else if (isGrounded)
            canDash = true;

        // Dash Ghost Timer
        if (isDashing)
        {
            // --- ADDED ---
            // Handle ghost spawning timer
            if (ghostSpawnTimer > 0)
            {
                ghostSpawnTimer -= Time.deltaTime;
            }

            // We only want dash cooldown to tick down 

            return;
        }
        
        
    }

    private void FixedUpdate()
    {
        Debug.Log("facingDirection: " + facingDirection);
        Debug.Log("horizontalInput: " + horizontalInput);

        if (wallBounceLockoutTimeCounter > 0f)
            return;

        if (!isDashing && !justWallBounced)
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        bool canGroundJump = coyoteTimeCounter > 0f;
        bool tryBufferJump = jumpBufferTimeCounter > 0f;
        // bool canWallBounce = wallBounceGraceTimeCounter > 0f;


        if (isDashing)
        {
            if (ghostSpawnTimer <= 0)
            {
                SpawnGhost();
                ghostSpawnTimer = ghostSpawnRate;
            }

            if (isTouchingWall && !isGrounded)
            {
                rb.linearVelocity = new Vector2(-facingDirection * wallBounceForce.x, wallBounceForce.y);
                wallBounceLockoutTimeCounter = wallBounceLockoutTime;
                coyoteTimeCounter = 0f;
                jumpBufferTimeCounter = 0f;
                // wallBounceGraceTimeCounter = 0f;
                jumpPressed = false;
                isDashing = false;
                canDash = true;
                justWallBounced = true;
                facingDirection *= -1f;
                rb.gravityScale = originalGravityScale;
                dashDurationCounter = 0f;
                // dashCooldownCounter = 0.1f;
            }

            dashDurationCounter -= Time.deltaTime;
            if (dashDurationCounter <= 0f)
            {
                isDashing = false;
                rb.gravityScale = originalGravityScale;
            }
            return; // Skip the rest of FixedUpdate while dashing
        }

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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
    }

    private void HandleChecks()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
        // isTouchingWall = Physics2D.Raycast(wallCheck.position, new Vector2(facingDirection, 0), wallCheckDistance, groundLayer);

        // Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckSize, Color.green);
        // Debug.DrawRay(wallCheck.position, new Vector2(facingDirection, 0) * wallCheckDistance, Color.red);

        if (isGrounded)
        {
            playerCollider.sharedMaterial = stickyMaterial;
            justWallBounced = false;
        }
        else
        {
            playerCollider.sharedMaterial = slippyMaterial;
        }

        if (isTouchingWall)
        {
            justWallBounced = false;
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
        bool isWallAttach = isTouchingWall && !isGrounded;
        anim.SetBool("isWallAttach", isWallAttach);
    }

    private void Flip()
    {
        if (wallBounceLockoutTimeCounter <= 0f && horizontalInput != 0 && !justWallBounced)
            facingDirection = horizontalInput;

        transform.localScale = new Vector3(-facingDirection, 1, 1);
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

    public void OnDash(InputValue value)
    {
        dashPressed = value.isPressed;

        if (dashPressed && canDash)
        {
            isDashing = true;
            canDash = false;
            dashDurationCounter = dashDuration;
            dashCooldownCounter = dashCooldown;

            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(facingDirection * dashForce, 0f);
        }
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

    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerSprite == null)
        {
            Debug.LogWarning("Ghost Prefab or Player Sprite is not assigned!");
            return;
        }
        
        // Create the ghost
        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        
        // Match the flip
        ghost.transform.localScale = transform.localScale;
        
        // Set the ghost's sprite to the player's *current* sprite
        ghost.GetComponent<SpriteRenderer>().sprite = playerSprite.sprite;
        
        // (The GhostFade.cs script on the prefab will handle the rest)
    }


    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        // Draw the ground check box
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }

}

