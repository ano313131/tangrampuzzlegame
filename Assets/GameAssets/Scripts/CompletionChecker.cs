using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletionChecker : MonoBehaviour
{
    private Piece[] pieces;
    private LevelGenerator levelGenerator;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    public void CheckForLevelCompletion()
    {
        bool isAllConnected = true;
        pieces = FindObjectsOfType<Piece>();

        foreach (var piece in pieces)
        {
            if (!piece.isConnected)
            {
                isAllConnected = false;
            }
                
        }

        if (isAllConnected)
        {
            StartNewLevel();
        }
            
    }

    private void StartNewLevel()
    {
        canvas.gameObject.SetActive(true);

    }
}
