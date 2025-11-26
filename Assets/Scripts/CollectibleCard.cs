using UnityEngine;

public class CollectibleCard : MonoBehaviour
{
    [Header("Card Settings")]
    public string cardID;          
    public int shardAmount = 1;   
    public int shardNeeded = 4;   
    public bool destroySelfOnHit = true;
    public int shieldAmount = 1;

    [Header("Floating Animation")]
    public float floatAmplitude = 0.25f;     // tinggi naik-turun
    public float floatFrequency = 2f;        // cepat naik-turun
    public float rotationSpeed = 0f;         // opsional: 0 = tidak berputar

    private Vector3 startPos;

    private void Awake()
    {
        // Jika kartu sudah unlocked permanen, opsional untuk hide
        if (GameManager.Instance != null && GameManager.Instance.data != null)
        {
            if (GameManager.Instance.data.collectible.Contains(cardID))
            {
                // Destroy(gameObject);  // jika ingin hilang selamanya
            }
        }
    }

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        // Floating naik turun
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = new Vector3(
            startPos.x,
            startPos.y + offsetY,
            startPos.z
        );

        // Opsional: spin
        if (rotationSpeed != 0f)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Tambah shard ke progress level ini
        GameManager.Instance.AddTempShard(cardID, shardAmount, shardNeeded);

        // Shield logic
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.GainShield(shieldAmount);

            if (LevelCardTracker.Instance != null)
            {
                LevelCardTracker.Instance.ReportCardCollected();
            }
        }

        if (destroySelfOnHit)
        {
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
        }
    }
}
