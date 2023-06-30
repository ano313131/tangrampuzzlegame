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
        levelGenerator.GenerateLevel(4);
        pieceGenerator.GeneratePieces(Random.Range(5, 7));
        canvas.gameObject.SetActive(false);
    }

    public void CreateMediumLevel()
    {
        DeleteLastLevel();
        levelGenerator.GenerateLevel(5);
        pieceGenerator.GeneratePieces(Random.Range(7, 9));
        canvas.gameObject.SetActive(false);
    }

    public void CreateHardLevel()
    {
        DeleteLastLevel();
        levelGenerator.GenerateLevel(6);
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
