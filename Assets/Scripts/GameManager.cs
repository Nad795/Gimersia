using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SaveData data;

    private void Awake()
    {
        Debug.Log(Application.persistentDataPath);

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

    // Cari progres shard untuk cardId tertentu di tempShards
    public CardShardProgress GetTempShardProgress(string cardId)
    {
        return data.tempShards.Find(s => s.cardId == cardId);
    }

    public void AddTempShard(string cardId, int amount, int shardNeededToUnlock = 4)
    {
        // Kalau already full card, tidak perlu nambah shard lagi
        if (data.collectible.Contains(cardId))
            return;

        var prog = GetTempShardProgress(cardId);
        if (prog == null)
        {
            prog = new CardShardProgress { cardId = cardId, shards = 0 };
            data.tempShards.Add(prog);
        }

        prog.shards += amount;

        // Kalau kamu mau langsung unlock begitu shard >= 4 di dalam level (tanpa nunggu menang),
        // bisa cek di sini.
        if (prog.shards >= shardNeededToUnlock)
        {
            // Pindahkan ke collectible full
            if (!data.collectible.Contains(cardId))
            {
                data.collectible.Add(cardId);
            }

            // (opsional) hapus dari tempShards
            data.tempShards.Remove(prog);
        }
    }
}
