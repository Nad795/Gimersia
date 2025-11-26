using System.Collections;
using UnityEngine;

public class Ghosthand : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float idleTime = 2f;      // How long it waits in the "Dangerous" state
    [SerializeField] private float inactiveTime = 2f;  // How long it stays underground
    
    [Header("Animation Durations")]
    [SerializeField] private float introDuration = 0.5f; // Length of your "Appear" clip
    [SerializeField] private float outroDuration = 0.5f; // Length of your "Disappear" clip
    [SerializeField] private float attackDuration = 0.35f;// Length of your "Attack" clip

    [Header("Wave Settings")]
    [SerializeField] private bool usePositionForTiming = true;
    [SerializeField] private float waveSpeed = 0.5f;

    [Header("Punishment")]
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 5f);

    [Header("Auto-Align")]
    [SerializeField] private bool autoAlignToSurfaces = true;
    [SerializeField] private LayerMask surfaceLayer;

    private Collider2D trapCollider;
    private Animator anim;
    private Coroutine currentRoutine;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (autoAlignToSurfaces)
        {
            AlignToSurface();
        }

        // Start with collider OFF so it can't hurt anyone yet
        trapCollider.enabled = false;

        float initialDelay = 0f;
        if (usePositionForTiming)
        {
            // Calculate total cycle time to keep the wave smooth
            float totalCycle = introDuration + idleTime + outroDuration + inactiveTime;
            initialDelay = (Mathf.Abs(transform.position.x) * waveSpeed) % totalCycle;
        }

        currentRoutine = StartCoroutine(CycleTrapRoutine(initialDelay));
    }

    private IEnumerator CycleTrapRoutine(float startDelay)
    {
        // 1. Initial Wave Delay
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // --- PHASE 1: APPEAR (Harmless) ---
            anim.SetTrigger("Appear");
            
            // Wait for the animation to finish rising out of the ground
            yield return new WaitForSeconds(introDuration);


            // --- PHASE 2: IDLE (Dangerous) ---
            // NOW we enable the collider. The hand is fully out and waiting.
            trapCollider.enabled = true;
            
            // Wait for the idle duration
            yield return new WaitForSeconds(idleTime);


            // --- PHASE 3: DISAPPEAR (Harmless) ---
            // Disable collider BEFORE it starts going back down
            trapCollider.enabled = false;
            
            anim.SetTrigger("Disappear");
            
            // Wait for the animation to finish going into the ground
            yield return new WaitForSeconds(outroDuration);


            // --- PHASE 4: INACTIVE (Underground) ---
            yield return new WaitForSeconds(inactiveTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Calculate Knockback based on rotation (transform.up)
                Vector2 direction = transform.up; 
                player.ApplyStun(stunDuration, direction * knockbackForce.magnitude);

                HandleAttack();
            }
        }
    }

    private void HandleAttack()
    {
        // 1. Interrupt the main loop so it doesn't try to Disappear while attacking
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        // 2. Play Attack Animation
        anim.SetTrigger("Attack");
        
        // 3. Disable collider immediately (so we don't hit the player twice)
        trapCollider.enabled = false;

        // 4. Start the reset logic
        currentRoutine = StartCoroutine(ResetAfterAttack());
    }

    private void AlignToSurface()
    {
        float checkDist = 1.2f; 


        if (Physics2D.Raycast(transform.position, Vector2.right, checkDist, surfaceLayer))
        {
            return;
        }
        
        if (Physics2D.Raycast(transform.position, Vector2.down, checkDist, surfaceLayer))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            return;
        }

        if (Physics2D.Raycast(transform.position, Vector2.left, checkDist, surfaceLayer))
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            spriteRenderer.flipY = true;
            return;
        }

        if (Physics2D.Raycast(transform.position, Vector2.up, checkDist, surfaceLayer))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            return;
        }

    }

    private IEnumerator ResetAfterAttack()
    {
        // 1. Wait for the attack animation to finish hitting the player
        yield return new WaitForSeconds(attackDuration);

        // 2. Play the Disappear animation (Going back into ground)
        anim.SetTrigger("Disappear");

        // 3. Wait for the Disappear animation to finish
        yield return new WaitForSeconds(outroDuration);

        // 4. Now stay underground for the inactive time
        yield return new WaitForSeconds(inactiveTime);

        // 5. Restart the loop fresh
        currentRoutine = StartCoroutine(CycleTrapRoutine(0f));
    }
}