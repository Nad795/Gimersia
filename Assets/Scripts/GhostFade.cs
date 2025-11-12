using UnityEngine;

/*
 * Attach this script to your "Ghost" prefab.
 * It automatically grabs the SpriteRenderer and fades it out over 'fadeTime',
 * then destroys the object.
 */
[RequireComponent(typeof(SpriteRenderer))]
public class GhostFade : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.5f;
    private SpriteRenderer spriteRenderer;
    private float fadeTimer;
    private Color startColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        fadeTimer = fadeTime;
    }

    void Update()
    {
        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            
            // Calculate new alpha
            float alpha = fadeTimer / fadeTime;
            
            // Set the new color
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha * startColor.a);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}