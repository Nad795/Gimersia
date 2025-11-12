using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NextLevel : MonoBehaviour
{
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
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float startOffsetX = -1000f;
    [SerializeField] private float endX = 0f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Beat Pauses (seconds, unscaled)")]
    [SerializeField] private float pauseAfterAnimAndSfx = 0.15f;
    [SerializeField] private float pauseAfterPanelSlide = 0.15f;

    private bool triggered;

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

        StartCoroutine(PlayWinSequence(playerInput));
    }

    private IEnumerator PlayWinSequence(PlayerInput playerInput)
    {
        // 1) Start animation + SFX simultaneously
        if (doorAnimator != null && !string.IsNullOrEmpty(winTrigger))
            doorAnimator.SetTrigger(winTrigger);

        if (winSfxSource != null && winSfx != null)
            winSfxSource.PlayOneShot(winSfx);

        yield return null; // give animator one frame to update

        // 2) Wait for both door animation and SFX to finish
        float animLen = GetCurrentOrNextClipLength(doorAnimator);
        float sfxLen = (winSfx != null ? winSfx.length : 0f) / (winSfxSource != null ? Mathf.Max(0.0001f, winSfxSource.pitch) : 1f);
        float waitLen = Mathf.Max(animLen, sfxLen);

        yield return new WaitForSecondsRealtime(waitLen + pauseAfterAnimAndSfx);

        // 3) Slide in the victory panel
        if (victoryPanel != null)
        {
            victoryPanel.gameObject.SetActive(true);
            Vector2 pos = victoryPanel.anchoredPosition;
            pos.x = startOffsetX;
            victoryPanel.anchoredPosition = pos;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, slideDuration);
                float k = easeCurve.Evaluate(Mathf.Clamp01(t));
                pos.x = Mathf.Lerp(startOffsetX, endX, k);
                victoryPanel.anchoredPosition = pos;
                yield return null;
            }

            victoryPanel.anchoredPosition = new Vector2(endX, victoryPanel.anchoredPosition.y);
        }

        // 4) Small pause before showing the button
        yield return new WaitForSecondsRealtime(pauseAfterPanelSlide);

        if (nextLevelButton != null)
            nextLevelButton.SetActive(true);
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
