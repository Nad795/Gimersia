using System.Collections;
using System.Dynamic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip jumpSfx;

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

    [Header("Stunning")]
    // [SerializeField] private float stunDuration = 2f;
    private bool isStunned;
    private bool isDashStunned;
    // private float stunTimer;

    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial2D slippyMaterial;
    [SerializeField] private PhysicsMaterial2D stickyMaterial;

    [Header("Run Smoke VFX")]
    [SerializeField] private ParticleSystem runSmokeVFX;

    [Header("UI")]
    [SerializeField] private GraphicRaycaster uiRaycaster;

    private void Awake()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        playerSprite = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale;

        if (uiRaycaster == null)
            uiRaycaster = FindObjectOfType<GraphicRaycaster>();
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
            if (ghostSpawnTimer > 0)
                ghostSpawnTimer -= Time.deltaTime;

            return;
        }
    }

    private void FixedUpdate()
    {
        if (wallBounceLockoutTimeCounter > 0f || isStunned)
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

            dashDurationCounter -= Time.deltaTime;
            if (dashDurationCounter <= 0f)
            {
                isDashing = false;
                rb.gravityScale = originalGravityScale;
            }
            return; // Skip the rest of FixedUpdate while dashing
        }

        // Wall Jump - requires jump input while touching wall and not grounded
        if (jumpPressed && isTouchingWall && !isGrounded)
        {
            rb.linearVelocity = new Vector2(-facingDirection * wallBounceForce.x, wallBounceForce.y);
            wallBounceLockoutTimeCounter = wallBounceLockoutTime;
            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            canDash = true;
            facingDirection *= -1f;
            justWallBounced = true;
            jumpPressed = false;

            if (sfxSource != null && jumpSfx != null)
                sfxSource.PlayOneShot(jumpSfx);
        }
        // Ground Jump
        else if (jumpPressed && canGroundJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (sfxSource != null && jumpSfx != null)
                sfxSource.PlayOneShot(jumpSfx);

            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            jumpPressed = false;
        }
        else if (tryBufferJump && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (sfxSource != null && jumpSfx != null)
                sfxSource.PlayOneShot(jumpSfx);

            jumpBufferTimeCounter = 0f;
            jumpPressed = false;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
    }

    private void HandleChecks()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (!isStunned)
        {
            isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
        }
        else
        {
            isTouchingWall = false;
        }

        if (isGrounded)
        {
            playerCollider.sharedMaterial = stickyMaterial;
            justWallBounced = false;
            isDashStunned = false;
        }
        else
        {
            playerCollider.sharedMaterial = slippyMaterial;
        }

        if (isTouchingWall)
            justWallBounced = false;
    }

    private void HandleAnimations()
    {
        // --- RUN ---
        bool isMoving = horizontalInput != 0 && isGrounded;
        anim.SetBool("isMoving", isMoving);

        HandleRunSmokeVFX(isMoving);

        // --- JUMP ---
        bool isJumping = !isGrounded && !isTouchingWall;
        anim.SetBool("isJumping", isJumping);

        // --- WALL ATTACH ---
        bool isWallAttach = (isTouchingWall && !isGrounded) || isStunned;
        anim.SetBool("isWallAttach", isWallAttach);
    }

    private void HandleRunSmokeVFX(bool isMoving)
    {
        if (runSmokeVFX == null) return;

        bool shouldPlay =
            isMoving &&
            !isDashing &&
            !isStunned;

        if (shouldPlay)
        {
            if (!runSmokeVFX.isPlaying)
                runSmokeVFX.Play();
        }
        else
        {
            if (!runSmokeVFX.isStopped)
                runSmokeVFX.Stop();
        }
    }

    private void Flip()
    {
        if (wallBounceLockoutTimeCounter <= 0f && horizontalInput != 0 && !justWallBounced)
            facingDirection = horizontalInput;

        transform.localScale = new Vector3(-facingDirection, 1, 1);
    }

    public void OnMove(InputValue value)
    {
        // Ignore gameplay input while paused (prevents stick drift affecting state)
        if (Time.timeScale == 0f) { moveInput = Vector2.zero; return; }
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        // Langsung (tanpa queue), tapi pakai raycast UI custom (tidak pakai IsPointerOverGameObject)
        if (Time.timeScale == 0f || IsPointerOverUI()) return;

        jumpHeld = value.isPressed;
        if (jumpHeld) jumpPressed = true;
        else          jumpPressed = false;
    }

    public void OnDash(InputValue value)
    {
        // Ignore dash when paused or clicking UI
        if (Time.timeScale == 0f || IsPointerOverUI()) return;

        dashPressed = value.isPressed;

        if (dashPressed && canDash && !isDashStunned)
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
        healthSystem.ActivateGameOverPanel();
    }

    public void ApplyStun(float stunDuration, Vector2 knockbackForce)
    {
        if (isStunned) return; // Prevent overlapping stuns

        StartCoroutine(StunRoutine(stunDuration, knockbackForce));
    }

    private IEnumerator StunRoutine(float stunDuration, Vector2 knockbackForce)
    {
        CancelDash();

        isStunned = true;
        isDashStunned = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);

        // Optionally, you can add visual feedback for being stunned here

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
    }

    private void CancelDash()
    {
        if (!isDashing) return;

        isDashing = false;
        dashPressed = false;
        dashDurationCounter = 0f;
        rb.gravityScale = originalGravityScale;
        ghostSpawnTimer = 0f;
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerSprite == null)
        {
            // Debug.LogWarning("Ghost Prefab or Player Sprite is not assigned!");
            return;
        }

        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        ghost.transform.localScale = transform.localScale;
        ghost.GetComponent<SpriteRenderer>().sprite = playerSprite.sprite;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }

    // ----- UI hit-test helper (mouse & touch) -----
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null || uiRaycaster == null)
            return false;

        // Siapkan event data
        var eventData = new PointerEventData(EventSystem.current);
        Vector2 pointerPos;

        // Cek touch dulu (mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // Kalau tidak ada touch, pakai mouse (PC/editor)
        else if (Mouse.current != null)
        {
            pointerPos = Mouse.current.position.ReadValue();
        }
        else
        {
            return false;
        }

        eventData.position = pointerPos;

        // Raycast ke semua UI yang ada di Canvas dengan GraphicRaycaster
        var results = new List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        return results.Count > 0;
    }
}
