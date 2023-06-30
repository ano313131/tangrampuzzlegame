using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletionChecker : MonoBehaviour
{
    private ConnectionPoint[] gridConnectionPoints;
    private LevelGenerator levelGenerator;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    public void CheckForLevelCompletion()
    {
        bool isAllConnected = true;
        gridConnectionPoints = GameObject.Find("ConnectionPoints").GetComponentsInChildren<ConnectionPoint>();

        foreach (var point in gridConnectionPoints)
        {
            if (!point.gameObject.GetComponent<CircleCollider2D>().IsTouchingLayers(LayerMask.GetMask("Piece")))
            {
                isAllConnected = false;
                Debug.Log("zort");
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
