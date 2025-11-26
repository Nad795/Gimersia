using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardShardProgress
{
    public string cardId;
    public int shards;
}

[System.Serializable]
public class SaveData
{
    public int level;

    // Kartu yang SUDAH FULL unlock
    public List<string> collectible = new List<string>();

    // Kartu yang dapat shard sementara di level ini (baru fix kalau menang)
    public List<CardShardProgress> tempShards = new List<CardShardProgress>();
}
