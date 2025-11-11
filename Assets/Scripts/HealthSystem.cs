using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lava"))
        {
            Debug.Log("Player touched lava! Instant death.");

            // Stop the rising lava
            RisingLava lava = FindObjectOfType<RisingLava>();
            if (lava != null)
                lava.StopLava();

            // Reload the scene or trigger your game over logic
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
