using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "LevelsData", menuName = "Scriptable Objects/LevelsData")]
public class LevelsDataManager : ScriptableObject
{
    [SerializeField] private List<LevelData> levels = new List<LevelData>();

    internal int TotalLevels => levels.Count;

    internal LevelData FetchLevelData(int levelNo)
    {
        return levels[levelNo];
    }
}



[System.Serializable]
public enum LevelTheme
{
    None,
    Forest,
    Desert,
    Snow,
    City,
    Space,
    Underwater
}


[System.Serializable]
public class LevelData
{
    public LevelTheme theme;
    public int AutoSpwnCount;

    public int[] TypeOfObjectTsSpawn;

    public float timeLimit; // in seconds
}