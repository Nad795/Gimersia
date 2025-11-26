using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SaveData data;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (SaveSystem.IsSaveExists())
            data = SaveSystem.LoadGame();
        else
            data = new SaveData();
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(data);
    }
}
