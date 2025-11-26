using UnityEngine;

public class CardAchievementSlot : MonoBehaviour
{
    [Header("ID kartu (harus sama dengan cardID di CollectibleCard)")]
    public string cardId;

    [Header("UI Objects")]
    public GameObject lockedObject;    // mis: Card1_Locked
    public GameObject unlockedObject;  // mis: Card1_Unlocked

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        // Safety check
        if (GameManager.Instance == null || GameManager.Instance.data == null)
            return;

        bool isUnlocked = GameManager.Instance.data.collectible.Contains(cardId);

        if (lockedObject != null)
            lockedObject.SetActive(!isUnlocked);

        if (unlockedObject != null)
            unlockedObject.SetActive(isUnlocked);
    }
}
