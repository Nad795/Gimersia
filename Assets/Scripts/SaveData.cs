using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int level;
    public List<string> collectible = new List<string>();
    public List<string> tempCollect = new List<string>();
}
