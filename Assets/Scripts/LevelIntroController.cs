using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LevelIntroController : MonoBehaviour
{
    [Header("Player (opsional)")]
    [SerializeField] private PlayerInput playerInput; // boleh dikosongkan, nanti dicari otomatis

    [Header("Level Title Panel")]
    [SerializeField] private CanvasGroup levelTitleGroup;   // panel "LEVEL 1-1 THE SILENT DESCENT"
    [SerializeField] private float titleFadeInDuration = 0.6f;
    [SerializeField] private float titleHoldDuration   = 1.5f;
    [SerializeField] private float titleFadeOutDuration = 0.6f;

    [Header("New Challenge Panel (opsional)")]
    [SerializeField] private bool showChallengeThisLevel = false;
    [SerializeField] private CanvasGroup challengeGroup;   // panel challenge baru
    [SerializeField] private Button challengeCloseButton;  // tombol Close di panel challenge
    [SerializeField] private float challengeFadeInDuration  = 0.5f;
    [SerializeField] private float challengeFadeOutDuration = 0.3f;

    private bool challengeClosed = false;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();

        // Pastikan panel awalnya dalam keadaan tidak terlihat
        if (levelTitleGroup != null)
        {
            levelTitleGroup.gameObject.SetActive(true);
            levelTitleGroup.alpha = 0f;
        }

        if (challengeGroup != null)
        {
            challengeGroup.gameObject.SetActive(false); // baru aktif saat dibutuhkan
            challengeGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        // Matikan input player sementara (opsional)
        if (playerInput != null)
            playerInput.enabled = false;

        // === 1. LEVEL TITLE ===
        if (levelTitleGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(levelTitleGroup, 0f, 1f, titleFadeInDuration));
            yield return new WaitForSecondsRealtime(titleHoldDuration);
            yield return StartCoroutine(FadeCanvasGroup(levelTitleGroup, 1f, 0f, titleFadeOutDuration));
            levelTitleGroup.gameObject.SetActive(false);
        }

        // === 2. NEW CHALLENGE (HANYA LEVEL TERTENTU) ===
        if (showChallengeThisLevel && challengeGroup != null)
        {
            challengeClosed = false;

            // Aktifkan panel + tombol close
            challengeGroup.gameObject.SetActive(true);
            challengeGroup.alpha = 0f;

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.AddListener(OnChallengeCloseClicked);
                challengeCloseButton.interactable = true;
            }

            // Fade-in panel challenge
            yield return StartCoroutine(FadeCanvasGroup(challengeGroup, 0f, 1f, challengeFadeInDuration));

            // Tunggu sampai tombol close ditekan
            yield return new WaitUntil(() => challengeClosed);

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.RemoveListener(OnChallengeCloseClicked);
                challengeCloseButton.interactable = false;
            }

            // Fade-out panel challenge (kalau mau langsung hilang, bisa skip bagian ini)
            yield return StartCoroutine(FadeCanvasGroup(challengeGroup, 1f, 0f, challengeFadeOutDuration));
            challengeGroup.gameObject.SetActive(false);
        }

        // Nyalakan lagi input player setelah semua intro selesai
        if (playerInput != null)
            playerInput.enabled = true;
    }

    private void OnChallengeCloseClicked()
    {
        challengeClosed = true;
    }

    // Utility fade in/out
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        duration = Mathf.Max(0.0001f, duration);
        cg.alpha = from;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
    }
}
