using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    public PlayerController playerController;
    public GameObject gameOver;

    private void Awake()
    {
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }
    }
    private void Start()
    {
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }
    }
    private void OnEnable()
    {
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lava"))
        {
            if (playerController != null)
                playerController.Die();

            // Stop the rising lava
            RisingLava lava = FindObjectOfType<RisingLava>();
            if (lava != null)
                lava.StopLava();
        }
    }

    public void ActivateGameOverPanel()
    {
        if (gameOver != null)
        {
            Time.timeScale = 0;
            gameOver.SetActive(true);
        }
    }
}
