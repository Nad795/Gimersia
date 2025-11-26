using UnityEngine;

public class StartLava : MonoBehaviour
{
    public RisingLava lava;
    [SerializeField] private bool triggered;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            lava.StartRising();
            triggered = true;
        }
    }
}
