using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RisingLava : MonoBehaviour
{
    [Header("Rise Settings")]
    [Tooltip("Vertical speed in units per second.")]
    public float riseSpeed = 0.4f;

    private Rigidbody2D rb;
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
        if (!isRising || isStopped) return;

        Vector2 pos = rb.position;
        pos += Vector2.up * riseSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos);
    }

    public void StartRising()
    {
        isRising = true;
    }

    public void StopLava()
    {
        isStopped = true;
    }
}
