using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float destroyY = -10f;
    public int damageAmount = 1;
    public bool destroySelfOnHit = true;

    void Update()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("Meteor collided with " + other.name);
        // Check if the object we hit has a HealthSystem
        // This works better than Tags because it targets the capability, not the name.
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Meteor hit player");
            HealthSystem health = other.GetComponent<HealthSystem>();
            
            if (health != null)
            {
                health.TakeDamage(damageAmount);

                if (destroySelfOnHit)
                {
                    GetComponent<Collider2D>().enabled = false; 
                    Destroy(gameObject);
                }
            }
        }
    }
}
