using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    private SpriteRenderer playerSprite;

    public PlayerController playerController;
    public GameObject gameOver;
    public int life = 3;
    public int maxLife = 5;
    public int shieldAmount = 0;

    [SerializeField] private GameObject shieldVisual;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip loseSfx;

    [SerializeField] private AudioSource levelBgmSource;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    private bool isInvulnerable = false;

    private void Awake()
    {
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }

        playerSprite = GetComponent<SpriteRenderer>();
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
        if (other.CompareTag("Lava"))
        {
            life--;

            if (other.CompareTag("Meteor"))
                TakeDamage(1);

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
        // Debug.Log("About to take " + damage + " damage");

        if (isInvulnerable)
        {
            // Debug.Log("Currently invulnerable, no damage taken");
            return;
        }

        if (shieldAmount > 0)
        {
            // Debug.Log("Shield absorbed damage");

            shieldAmount--;
            
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(shieldAmount > 0);
            }

            StartCoroutine(InvulnerabilityCoroutine());

            return;
        }

        life -= damage;
        // Debug.Log("Took " + damage + " damage, life is now " + life);

        if (life <= 0)
        {

            // Debug.Log("Life has reached zero or below");
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

        StartCoroutine(InvulnerabilityCoroutine());
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
        DiscardTempCollectibles();
        
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

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            if (playerSprite != null)
            {
                playerSprite.enabled = !playerSprite.enabled;
            }
            
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
        }
        
        isInvulnerable = false;
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

    public void DiscardTempCollectibles()
    {
        if (GameManager.Instance == null || GameManager.Instance.data == null)
            return;

        // Jika tidak ada shard yang dikumpulkan di level ini → tidak usah discard
        if (GameManager.Instance.data.tempShards == null || GameManager.Instance.data.tempShards.Count == 0)
        {
            // Debug.Log("[HealthSystem] Tidak ada shard yang dikumpulkan. Tidak ada yang perlu di-discard.");
            return;
        }

        // Jika ada shard → hapus semuanya (karena player kalah)
        // Debug.Log("[HealthSystem] Player kalah. Menghapus tempShards yang terkumpul di level ini.");

        GameManager.Instance.data.tempShards.Clear();
        GameManager.Instance.SaveGame();
    }
}
