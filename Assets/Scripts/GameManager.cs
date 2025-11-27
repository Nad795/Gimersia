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

        // 🔒 PENTING: pastikan list tidak null (untuk save.json lama)
        if (data.collectible == null)
            data.collectible = new List<string>();

        if (data.tempShards == null)
            data.tempShards = new List<CardShardProgress>();
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(data);
    }

    // Cari progres shard untuk cardId tertentu di tempShards
    public CardShardProgress GetTempShardProgress(string cardId)
    {
        if (data == null || data.tempShards == null) return null;
        return data.tempShards.Find(s => s.cardId == cardId);
    }

    public void AddTempShard(string cardId, int amount, int shardNeededToUnlock = 4)
    {
        // Guard kalau data atau list masih null entah kenapa
        if (data == null)
        {
            // Debug.LogWarning("[GameManager] data null saat AddTempShard, inisialisasi ulang SaveData.");
            data = new SaveData();
        }

        if (data.collectible == null)
            data.collectible = new List<string>();
        if (data.tempShards == null)
            data.tempShards = new List<CardShardProgress>();

        // Kalau sudah full card, tidak perlu nambah shard lagi
        if (data.collectible.Contains(cardId))
            return;

        var prog = GetTempShardProgress(cardId);
        if (prog == null)
        {
            prog = new CardShardProgress { cardId = cardId, shards = 0 };
            data.tempShards.Add(prog);
        }

        prog.shards += amount;

        // Kalau shard sudah cukup untuk unlock
        if (prog.shards >= shardNeededToUnlock)
        {
            if (!data.collectible.Contains(cardId))
            {
                data.collectible.Add(cardId);
            }

            data.tempShards.Remove(prog);
        }

        SaveGame();
    }
}
