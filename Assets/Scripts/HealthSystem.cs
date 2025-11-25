using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    public PlayerController playerController;
    public GameObject gameOver;
    public int life = 3;
    public int maxLife = 5;
    public int shieldAmount = 0;

    [SerializeField] private GameObject shieldVisual;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip loseSfx;

    [SerializeField] private AudioSource levelBgmSource;

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

        //instantiate heart
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
        if (other.CompareTag("Lava") || other.CompareTag("Meteor"))
        {
            life--;

            if(life <= 0)
            {
                if (playerController != null)
                {
                    playerController.Die();
                }

                // Stop the rising lava
                RisingLava lava = FindAnyObjectByType<RisingLava>();
                if (lava != null)
                {
                    lava.StopLava();
                }

                MeteorSpawner meteor = FindAnyObjectByType<MeteorSpawner>();
                if (meteor != null)
                {
                    meteor.StopSpawning();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (shieldAmount > 0)
        {
            shieldAmount--;
            
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(shieldAmount > 0);
            }

            return;
        }

        life -= damage;

        if (life <= 0)
        {
            if (playerController != null)
            {
                playerController.Die();
            }

            // Stop the rising lava
            RisingLava lava = FindAnyObjectByType<RisingLava>();
            if (lava != null)
            {
                lava.StopLava();
            }

            MeteorSpawner meteor = FindAnyObjectByType<MeteorSpawner>();
            if (meteor != null)
            {
                meteor.StopSpawning();
            }
        }
    }

    public void GainShield(int amount)
    {
        shieldAmount += amount;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(shieldAmount > 0);
        }
    }

    public void ActivateGameOverPanel()
    {
        if (gameOver != null)
        {
            if (levelBgmSource != null)
                StartCoroutine(FadeOutAudio(levelBgmSource, 1f));
                
            if (sfxSource != null && loseSfx != null)
            {
                sfxSource.PlayOneShot(loseSfx);
            }
            Time.timeScale = 0;
            gameOver.SetActive(true);
        }
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration = 1f)
    {
        if (source == null || !source.isPlaying) yield break;

        float startVol = source.volume;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            source.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        source.Stop();
        source.volume = startVol; // reset for next use
    }
}
