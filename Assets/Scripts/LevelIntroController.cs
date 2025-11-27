using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelIntroController : MonoBehaviour
{
    [Header("Player (opsional)")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Level Title Panel")]
    [SerializeField] private CanvasGroup levelTitleGroup;
    [SerializeField] private float titleHoldDuration = 1.5f;

    [Header("New Challenge Panel (opsional)")]
    [SerializeField] private bool showChallengeThisLevel = false;
    [SerializeField] private CanvasGroup challengeGroup;
    [SerializeField] private Button challengeCloseButton;

    [Header("Meteor Control (opsional)")]
    [SerializeField] private MeteorSpawner[] meteorSpawners;

    private bool challengeClosed = false;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();

        if (meteorSpawners == null || meteorSpawners.Length == 0)
            meteorSpawners = FindObjectsOfType<MeteorSpawner>();

        if (levelTitleGroup != null)
        {
            levelTitleGroup.gameObject.SetActive(false);
            levelTitleGroup.alpha = 1f;
        }

        if (challengeGroup != null)
        {
            challengeGroup.gameObject.SetActive(false);
            challengeGroup.alpha = 1f;
        }
    }

    private void Start()
    {
        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        // matikan input & meteor
        if (playerInput != null)
            playerInput.enabled = false;

        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
                if (spawner != null) spawner.PauseSpawning();
        }

        // === LEVEL TITLE ===
        if (levelTitleGroup != null)
        {
            levelTitleGroup.gameObject.SetActive(true);
            levelTitleGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(titleHoldDuration);
            levelTitleGroup.gameObject.SetActive(false);
        }

        // === CHALLENGE PANEL ===
        if (showChallengeThisLevel && challengeGroup != null)
        {
            challengeClosed = false;

            challengeGroup.gameObject.SetActive(true);
            challengeGroup.alpha = 1f;

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.AddListener(OnChallengeCloseClicked);
                challengeCloseButton.interactable = true;

                // --------------- 🔥 PERUBAHAN PENTING #1 -----------------
                // Jika Level1, jangan sampai disabled
                if (SceneManager.GetActiveScene().name == "Level1")
                    challengeCloseButton.interactable = true;
                // ---------------------------------------------------------
            }

            // Tunggu tombol ditekan
            yield return new WaitUntil(() => challengeClosed);

            if (challengeCloseButton != null)
            {
                challengeCloseButton.onClick.RemoveListener(OnChallengeCloseClicked);

                // --------------- 🔥 PERUBAHAN PENTING #2 -----------------
                // Di Level1, JANGAN nonaktifkan tombol
                if (SceneManager.GetActiveScene().name != "Level1")
                    challengeCloseButton.interactable = false;
                // ---------------------------------------------------------
            }

            challengeGroup.gameObject.SetActive(false);
        }

        // aktifkan input & meteor lagi
        if (playerInput != null)
            playerInput.enabled = true;

        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
                if (spawner != null) spawner.ResumeSpawning();
        }
    }

    private void OnChallengeCloseClicked()
    {
        challengeClosed = true;
    }
}
