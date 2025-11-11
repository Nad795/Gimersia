using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RisingLava : MonoBehaviour
{
    [Header("Rise Settings")]
    [Tooltip("Delay before lava starts rising (seconds).")]
    public float startDelay = 3f;

    [Tooltip("Vertical speed in units per second.")]
    public float riseSpeed = 0.4f;

    private Rigidbody2D rb;
    private float timer;
    private bool isRising;
    private bool isStopped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 0;
    }

    private void FixedUpdate()
    {
        if (isStopped) return;  // Stop when player is dead

        // Wait before starting
        if (!isRising)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= startDelay)
                isRising = true;
            else
                return;
        }

        // Move upward forever
        Vector2 pos = rb.position;
        pos += Vector2.up * riseSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos);
    }

    public void StopLava()
    {
        isStopped = true;
    }
}
