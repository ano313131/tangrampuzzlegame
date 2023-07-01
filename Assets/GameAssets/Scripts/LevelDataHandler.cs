using System;
using System.IO;
using UnityEngine;

public class LevelDataHandler : MonoBehaviour
{
    public string SavePath => $"{Application.dataPath}/GameAssets/LevelData/levelData.json";
    public static LevelDataHandler Instance;

    private void Awake()
    {
        LevelDataHandler.Instance = this;
    }

    public void SaveLevelData(int gridSize, int pieceCount)
    {
        LevelData levelData = new LevelData
        {
            GridSize = gridSize,
            PieceCount = pieceCount,
        };

        string json = JsonUtility.ToJson(levelData);

        File.WriteAllText(SavePath, json);
    }

    public LevelData LoadLevelData()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogError("Save file not found!");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        return levelData;
    }
}

[System.Serializable]
public class LevelData
{
    public int GridSize;
    public int PieceCount;
}