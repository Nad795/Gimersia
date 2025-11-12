using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // ── Singleton & Persist ─────────────────────────────────────────────────────
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureFadeCanvas();
        // Start transparent (no fade at boot)
        SetAlpha(0f);
    }

    // ── Fade Settings ───────────────────────────────────────────────────────────
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color fadeColor = Color.black;

    // Runtime-created UI
    private Canvas _fadeCanvas;
    private Image  _fadeImage;

    // ── Public API (always fades) ───────────────────────────────────────────────
    public void ReloadCurrentScene()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(FadeAndLoad(() => SceneManager.LoadScene(idx)));
    }

    public void LoadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(FadeAndLoad(() => SceneManager.LoadScene(next)));
        else
            Debug.Log("No next scene found — reached the end of build index list.");
    }

    public void LoadPreviousScene()
    {
        int prev = SceneManager.GetActiveScene().buildIndex - 1;
        if (prev >= 0)
            StartCoroutine(FadeAndLoad(() => SceneManager.LoadScene(prev)));
        else
            Debug.Log("No previous scene — this is the first one.");
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(FadeAndLoad(() => SceneManager.LoadScene(index)));
        else
            Debug.LogError("Scene index out of range!");
    }

    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(FadeAndLoad(() => SceneManager.LoadScene(sceneName)));
    }

    public void QuitGame()
    {
        StartCoroutine(FadeAndLoad(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
    }

    // ── Core Fade Routine ───────────────────────────────────────────────────────
    private IEnumerator FadeAndLoad(System.Action loadAction)
    {
        // Fade to black
        yield return Fade(1f);
        // Do the load/action
        loadAction?.Invoke();
        // Wait a frame so new scene can render at least once
        yield return null;
        // Fade back in
        yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        EnsureFadeCanvas();

        float startAlpha = _fadeImage.color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            float k = fadeCurve.Evaluate(Mathf.Clamp01(t));
            float a = Mathf.Lerp(startAlpha, targetAlpha, k);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        if (_fadeImage == null) return;
        var c = fadeColor; c.a = Mathf.Clamp01(a);
        _fadeImage.color = c;
        // Block input while fully/partially faded
        _fadeImage.raycastTarget = a > 0f;
    }

    // ── Create overlay canvas & image if missing ────────────────────────────────
    private void EnsureFadeCanvas()
    {
        if (_fadeCanvas != null && _fadeImage != null) return;

        // Canvas
        var goCanvas = new GameObject("SceneController_FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(goCanvas);
        _fadeCanvas = goCanvas.GetComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = short.MaxValue; // on top of everything

        // Image
        var goImage = new GameObject("FadeImage", typeof(Image));
        goImage.transform.SetParent(goCanvas.transform, false);
        _fadeImage = goImage.GetComponent<Image>();
        _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        _fadeImage.raycastTarget = false;

        // Stretch full screen
        var rt = _fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
