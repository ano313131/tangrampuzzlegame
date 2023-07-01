using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreateNewLevel : MonoBehaviour
{
    private LevelGenerator levelGenerator;
    private PieceGenerator pieceGenerator;
    [SerializeField] private Canvas canvas;
    private int gridSize;
    private int pieceCount;

    private void Awake()
    {
        levelGenerator = GetComponent<LevelGenerator>();
        pieceGenerator = GetComponent<PieceGenerator>();
    }

    private void Start()
    {
        CreateEasyLevel();
    }

    public void CreateEasyLevel()
    {
        DeleteLastLevel();
        
        gridSize = 4;
        pieceCount = Random.Range(5, 7);
        
        levelGenerator.GenerateLevel(gridSize);
        pieceGenerator.GeneratePieces(pieceCount);
        canvas.gameObject.SetActive(false);
        
    }

    public void CreateMediumLevel()
    {
        DeleteLastLevel();
        
        gridSize = 5;
        pieceCount = Random.Range(7, 9);
        
        levelGenerator.GenerateLevel(gridSize);
        pieceGenerator.GeneratePieces(pieceCount);
        canvas.gameObject.SetActive(false);
        
    }

    public void CreateHardLevel()
    {
        DeleteLastLevel();
        
        gridSize = 6;
        pieceCount = Random.Range(9, 12);
        
        levelGenerator.GenerateLevel(gridSize);
        pieceGenerator.GeneratePieces(Random.Range(9, 12));
        canvas.gameObject.SetActive(false);
        
    }

    void DeleteLastLevel()
    {
        var levelObj = GameObject.FindGameObjectWithTag("Level");
        if(levelObj)
            DestroyImmediate(levelObj.transform.gameObject);
    }
}
