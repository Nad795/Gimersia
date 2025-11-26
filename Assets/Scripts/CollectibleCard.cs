using UnityEngine;

public class CollectibleCard : MonoBehaviour
{
    public string cardID;
    public bool destroySelfOnHit = true;
    public int shieldAmount = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Card collided with " + other.name);
        // Check if the object we hit has a HealthSystem
        // This works better than Tags because it targets the capability, not the name.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Card collected by player");
            
            if (!GameManager.Instance.data.tempCollect.Contains(cardID))
                GameManager.Instance.data.tempCollect.Add(cardID);

            HealthSystem health = other.GetComponent<HealthSystem>();
            
            if (health != null)
            {
                health.GainShield(shieldAmount);

                if (LevelCardTracker.Instance != null)
                {
                    LevelCardTracker.Instance.ReportCardCollected();
                }

                if (destroySelfOnHit)
                {
                    GetComponent<Collider2D>().enabled = false; 
                    Destroy(gameObject);
                }
            }
        }
    }
}
