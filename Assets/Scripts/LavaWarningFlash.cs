using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LavaWarningFlash : MonoBehaviour
{
    private Image img;

    [Header("Flash Settings")]
    public float flashDuration = 0.75f;
    public int flashCount = 1;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);

    private void Awake()
    {
        img = GetComponent<Image>();
        img.color = new Color(1, 0, 0, 0);
    }

    public IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // Fade in
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / (flashDuration / 2f);
                img.color = Color.Lerp(new Color(1, 0, 0, 0), flashColor, t);
                yield return null;
            }

            // Fade out
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / (flashDuration / 2f);
                img.color = Color.Lerp(flashColor, new Color(1, 0, 0, 0), t);
                yield return null;
            }
        }
    }
}
