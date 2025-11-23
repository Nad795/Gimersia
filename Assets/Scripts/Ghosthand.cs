using System.Collections;
using UnityEngine;

public class Ghosthand : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float inactiveTime = 2f;
    [SerializeField] private bool usePositionForTiming = true; // Creates a "wave" effect
    [SerializeField] private float waveSpeed = 0.5f; 

    [Header("Punishment")]
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 5f);

    [Header("Visuals")]
    [SerializeField] private float fadeSpeed = 5f;

    private Collider2D trapCollider;
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // If we use position timing, the hands won't all appear at once.
        // They will appear in a sequence based on their X position.
        float initialDelay = 0f;
        if (usePositionForTiming)
        {
            // We use absolute value so negative x positions don't break logic
            initialDelay = (Mathf.Abs(transform.position.x) * waveSpeed) % (activeTime + inactiveTime);
        }

        StartCoroutine(CycleTrapRoutine(initialDelay));
    }

    private IEnumerator CycleTrapRoutine(float startDelay)
    {
        // Initial state: Invisible and harmless
        isActive = false;
        trapCollider.enabled = false;
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        // Wait for the calculated wave offset
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // --- BECOME ACTIVE ---
            isActive = true;
            trapCollider.enabled = true;
            yield return FadeAlpha(1f); 
            
            yield return new WaitForSeconds(activeTime);

            // --- BECOME INACTIVE ---
            isActive = false;
            trapCollider.enabled = false;
            yield return FadeAlpha(0f); 
            
            yield return new WaitForSeconds(inactiveTime);
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha)
    {
        Color color = spriteRenderer.color;
        // Tiny optimization: Don't run loop if already close
        while (Mathf.Abs(color.a - targetAlpha) > 0.05f)
        {
            color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
            spriteRenderer.color = color;
            yield return null;
        }
        color.a = targetAlpha;
        spriteRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                Vector2 knockback = Vector2.Scale(direction, knockbackForce);
                player.ApplyStun(stunDuration, knockback);
            }
        }
    }
}