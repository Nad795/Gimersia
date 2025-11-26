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
    [SerializeField] private float titleHoldDuration = 1.5f;

    [Header("New Challenge Panel (opsional)")]
    [SerializeField] private bool showChallengeThisLevel = false;
    [SerializeField] private CanvasGroup challengeGroup;   // panel challenge baru
    [SerializeField] private Button challengeCloseButton;  // tombol Close di panel challenge

    [Header("Meteor Control (opsional)")]
    [SerializeField] private MeteorSpawner[] meteorSpawners; // kalau kosong, akan dicari otomatis

    private bool challengeClosed = false;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();

        if (meteorSpawners == null || meteorSpawners.Length == 0)
            meteorSpawners = FindObjectsOfType<MeteorSpawner>();

        // Panel level title: awalnya nonaktif
        if (levelTitleGroup != null)
        {
            levelTitleGroup.gameObject.SetActive(false);
            levelTitleGroup.alpha = 1f; // tidak pakai fade, langsung full
        }

        // Panel challenge: awalnya nonaktif
        if (challengeGroup != null)
        {
            challengeGroup.gameObject.SetActive(false);
            challengeGroup.alpha = 1f;  // langsung full
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

        // Pause jatuhnya meteor
        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
            {
                if (spawner != null)
                    spawner.PauseSpawning();
            }
        }

        // === 1. LEVEL TITLE === (tanpa fade: langsung muncul, tunggu, lalu hilang)
        if (levelTitleGroup != null)
        {
            levelTitleGroup.gameObject.SetActive(true);
            levelTitleGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(titleHoldDuration);

            levelTitleGroup.gameObject.SetActive(false);
        }

        // === 2. NEW CHALLENGE (HANYA LEVEL TERTENTU) ===
        if (showChallengeThisLevel && challengeGroup != null)
        {
            challengeClosed = false;

            challengeGroup.gameObject.SetActive(true);
            challengeGroup.alpha = 1f;

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.AddListener(OnChallengeCloseClicked);
                challengeCloseButton.interactable = true;
            }

            // Tunggu sampai tombol close ditekan
            yield return new WaitUntil(() => challengeClosed);

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.RemoveListener(OnChallengeCloseClicked);
                challengeCloseButton.interactable = false;
            }

            // Tanpa fade, langsung hilang
            challengeGroup.gameObject.SetActive(false);
        }

        // Nyalakan lagi input player setelah semua intro selesai
        if (playerInput != null)
            playerInput.enabled = true;

        // Resume jatuhnya meteor
        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
            {
                if (spawner != null)
                    spawner.ResumeSpawning();
            }
        }
    }

    private void OnChallengeCloseClicked()
    {
        challengeClosed = true;
    }
}
