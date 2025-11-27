using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] public MeteorSpawner meteorSpawner;

    [Header("Player Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Animation & Audio")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string winTrigger = "Win";
    [SerializeField] private AudioSource winSfxSource;
    [SerializeField] private AudioClip winSfx;
    [SerializeField] private AudioSource levelBgmSource;

    [Header("Victory UI")]
    [SerializeField] private RectTransform victoryPanel;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float startOffsetX = -1000f;
    [SerializeField] private float endX = 0f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Virtue Art")]
    [SerializeField] private RectTransform virtueArtPanel;
    [SerializeField] private GameObject virtueArtButton;
    [SerializeField] private float virtueFadeDuration = 0.5f;
    private bool artDismissed = false;

    [Header("Beat Pauses (seconds, unscaled)")]
    [SerializeField] private float pauseAfterAnimAndSfx = 0.15f;
    [SerializeField] private float pauseAfterPanelSlide = 0.15f;

    [Header("Level Progression")]
    [Tooltip("Nilai level tertinggi yang akan di-unlock ketika level ini selesai. Contoh: Level1 = 2, Level2 = 3, ..., Level9 = 9.")]
    [SerializeField] private int unlockLevelValue = 1;
    [Tooltip("Jumlah level maksimum yang ada di game (saat ini 9).")]
    [SerializeField] private int maxLevel = 9;

    private bool triggered;
    [SerializeField] private GameObject pauseButton;
    private bool levelSaved = false;

    private void Start()
    {
        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);
        
        if (restartButton != null)          
            restartButton.SetActive(false); 

        if (virtueArtPanel != null)
        {
            virtueArtPanel.gameObject.SetActive(false);

            var cg = virtueArtPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        if (victoryPanel != null)
            victoryPanel.gameObject.SetActive(false);

        if (virtueArtButton != null)
        {
            virtueArtButton.SetActive(false);
            virtueArtButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(DismissVirtueArt);
        }
    }

    public void DismissVirtueArt()
    {
        artDismissed = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        // Stop the rising lava
        RisingLava lava = FindObjectOfType<RisingLava>();
        if (lava != null)
            lava.StopLava();

        if (levelBgmSource != null)
            StartCoroutine(FadeOutAudio(levelBgmSource, 1f));

        // --- Disable all player input ---
        PlayerInput playerInput = other.GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = false;

        bool collectedAllCards = LevelCardTracker.Instance != null &&
                                 LevelCardTracker.Instance.HasCollectedAllCards();

        StartCoroutine(PlayWinSequence(collectedAllCards));
    }

    private IEnumerator PlayWinSequence(bool showSpecialArt)
    {
        var gm = GameManager.Instance;

        // ==== PERBAIKAN SAVE LEVEL DI SINI ====
        if (!levelSaved && gm != null && gm.data != null)
        {
            int clampedUnlock = Mathf.Clamp(unlockLevelValue, 1, maxLevel);

            // Hanya naik kalau unlockLevelValue lebih tinggi dari level yang sudah pernah dicapai
            gm.data.level = Mathf.Max(gm.data.level, clampedUnlock);
            gm.SaveGame();
            levelSaved = true;
        }

        if (pauseButton != null) pauseButton.SetActive(false);

        if (doorAnimator != null && !string.IsNullOrEmpty(winTrigger))
            doorAnimator.SetTrigger(winTrigger);

        if (winSfxSource != null && winSfx != null)
            winSfxSource.PlayOneShot(winSfx);

        string sceneName = SceneManager.GetActiveScene().name;

        if ((sceneName == "Level7" || sceneName == "Level8" || sceneName == "Level9") 
            && meteorSpawner != null)
        {
            meteorSpawner.StopSpawning();
        }

        yield return null;

        float animLen = GetCurrentOrNextClipLength(doorAnimator);
        float sfxLen = (winSfx != null ? winSfx.length : 0f) /
                       (winSfxSource != null ? Mathf.Max(0.0001f, winSfxSource.pitch) : 1f);
        float waitLen = Mathf.Max(animLen, sfxLen);

        yield return new WaitForSecondsRealtime(waitLen + pauseAfterAnimAndSfx);

        // --- Virtue art (fade-in) ---
        if (showSpecialArt && virtueArtPanel != null)
        {
            yield return StartCoroutine(FadeInPanel(virtueArtPanel, virtueFadeDuration));

            if (virtueArtButton != null)
                virtueArtButton.SetActive(true);

            yield return new WaitUntil(() => artDismissed);

            virtueArtButton.SetActive(false);
        }

        // --- Victory panel (slide) ---
        if (victoryPanel != null)
        {
            yield return StartCoroutine(SlidePanelIn(victoryPanel));
        }

        yield return new WaitForSecondsRealtime(pauseAfterPanelSlide);

        // Show the Scene Loader Button
        if (nextLevelButton != null)
            nextLevelButton.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);

        // Commit collectible shards hanya kalau GameManager valid
        if (gm != null && gm.data != null)
        {
            CommitCollectiblesOnWin();
        }
    }

    public void CommitCollectiblesOnWin()
    {
        if (GameManager.Instance == null || GameManager.Instance.data == null)
            return;

        if (GameManager.Instance.data.tempShards == null)
        {
            GameManager.Instance.data.tempShards = new List<CardShardProgress>();
            GameManager.Instance.SaveGame();
            return;
        }

        int shardNeededToUnlock = 4;

        foreach (var prog in GameManager.Instance.data.tempShards)
        {
            if (prog.shards >= shardNeededToUnlock)
            {
                if (!GameManager.Instance.data.collectible.Contains(prog.cardId))
                {
                    GameManager.Instance.data.collectible.Add(prog.cardId);
                }
            }
        }

        GameManager.Instance.data.tempShards.Clear();
        GameManager.Instance.SaveGame();
    }

    private IEnumerator SlidePanelIn(RectTransform panel)
    {
        panel.gameObject.SetActive(true);
        Vector2 pos = panel.anchoredPosition;
        pos.x = startOffsetX;
        panel.anchoredPosition = pos;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, slideDuration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            float k = easeCurve.Evaluate(Mathf.Clamp01(t));
            pos.x = Mathf.Lerp(startOffsetX, endX, k);
            panel.anchoredPosition = pos;
            yield return null;
        }

        panel.anchoredPosition = new Vector2(endX, panel.anchoredPosition.y);
    }

    private IEnumerator FadeInPanel(RectTransform panel, float duration)
    {
        if (panel == null) yield break;

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = panel.gameObject.AddComponent<CanvasGroup>();

        panel.gameObject.SetActive(true);
        cg.alpha = 0f;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        cg.alpha = 1f;
    }

    // Helper to get length of current/next animation clip
    private float GetCurrentOrNextClipLength(Animator anim)
    {
        if (anim == null) return 0f;

        var next = anim.GetNextAnimatorClipInfo(0);
        if (next != null && next.Length > 0)
            return next[0].clip.length / Mathf.Max(0.0001f, anim.speed);

        var curr = anim.GetCurrentAnimatorClipInfo(0);
        if (curr != null && curr.Length > 0)
            return curr[0].clip.length / Mathf.Max(0.0001f, anim.speed);

        return 0f;
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration = 1f)
    {
        if (source == null || !source.isPlaying) yield break;

        float startVol = source.volume;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            source.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        source.Stop();
        source.volume = startVol;
    }
}
