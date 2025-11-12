using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Audio")]
    [SerializeField] private AudioSource pauseSfxSource;
    [SerializeField] private AudioClip pauseSfx;

    private bool isPaused;
    private InputAction pauseAction;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput != null)
            pauseAction = playerInput.actions["Pause"];
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
            pauseAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }
    }

    private void Start()
    {
        if (pauseMenu)
            pauseMenu.SetActive(false);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    // === Public API ===
    public void TogglePause()
    {
        // Play pause SFX regardless of pause/resume
        if (pauseSfxSource != null && pauseSfx != null)
            pauseSfxSource.PlayOneShot(pauseSfx);

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenu)
            pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu)
            pauseMenu.SetActive(false);
    }
}
