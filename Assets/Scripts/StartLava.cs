using UnityEngine;
using System.Collections;

public class StartLava : MonoBehaviour
{
    public RisingLava lava;
    public LavaWarningFlash warningFlash;

    [SerializeField] private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(StartLavaRoutine());
        }
    }

    private IEnumerator StartLavaRoutine()
    {
        // Flash dulu
        if (warningFlash != null)
            yield return StartCoroutine(warningFlash.FlashRoutine());

        // Lava mulai naik
        lava.StartRising();
    }
}
