using UnityEngine;

public class CollectibleCard : MonoBehaviour
{
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
            HealthSystem health = other.GetComponent<HealthSystem>();
            
            if (health != null)
            {
                health.GainShield(shieldAmount);

                if (destroySelfOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
