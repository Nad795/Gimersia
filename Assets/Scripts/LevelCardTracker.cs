using UnityEngine;
using TMPro; // Include this if you want UI text

public class LevelCardTracker : MonoBehaviour
{
    public static LevelCardTracker Instance;

    [Header("Debug Info")]
    [SerializeField] private int totalCards;
    [SerializeField] private int collectedCards;

    // [Header("Optional UI")]
    // [SerializeField] private TextMeshProUGUI counterText; // Drag a UI Text here: "0/3"

    private void Awake()
    {
        // Simple Singleton setup for this scene
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Find all cards currently in the scene
        CollectibleCard[] cards = FindObjectsByType<CollectibleCard>(FindObjectsSortMode.None);
        totalCards = cards.Length;

        Debug.Log($"LevelCardTracker: Found {totalCards} collectible cards in the level.");

        // UpdateUI();
    }

    public void ReportCardCollected()
    {
        collectedCards++;
        // UpdateUI();
    }

    // private void UpdateUI()
    // {
    //     if (counterText != null)
    //     {
    //         counterText.text = $"{collectedCards} / {totalCards}";
    //     }
    // }

    public bool HasCollectedAllCards()
    {
        return totalCards > 0 && collectedCards >= totalCards;
    }
}