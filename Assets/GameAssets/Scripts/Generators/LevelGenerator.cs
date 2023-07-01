using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelGenerator : MonoBehaviour
{
    public GameObject circlePrefab;
    public GameObject gridCell;
    public GameObject wallPrefab;
    private int triangleMatrixSize;
    private GameObject level;

    public void GenerateLevel(float gridSize)
    {
        MakeLevelGameObject();
        triangleMatrixSize = (int)gridSize * 2 + 1; 
        GenerateTriangles();
        CenterTheCamera();
        GenerateGridPoints();
        MakeOutline(gridSize);

    }

    void MakeLevelGameObject()
    {
        level = new GameObject("Level");
        level.tag = "Level";
        level.AddComponent<SortingGroup>();
    }

    void GenerateTriangles()
    {
        for (int i = 0; i < triangleMatrixSize-1; i += 2)
        {
            for (int j = 0; j < triangleMatrixSize-1; j += 2)
            {
                CreateTriangleGameObject(new Vector3(i, j, 0), new Vector3(i + 1, j + 1, 0), new Vector3(i, j + 2, 0));
                CreateTriangleGameObject(new Vector3(i, j, 0), new Vector3(i + 1, j + 1, 0), new Vector3(i + 2, j, 0));
                CreateTriangleGameObject(new Vector3(i + 2, j, 0), new Vector3(i + 1, j + 1, 0), new Vector3(i + 2, j + 2, 0));
                CreateTriangleGameObject(new Vector3(i, j + 2, 0), new Vector3(i + 1, j + 1, 0), new Vector3(i + 2, j + 2, 0));
            }
        }
    }

    void GenerateGridPoints()
    {
        GameObject connectionPoints = new GameObject("ConnectionPoints");
        GameObject cellBackGrounds = new GameObject("CellBackGrounds");
        connectionPoints.transform.parent = level.transform;
        cellBackGrounds.transform.parent = level.transform;
        for (int i = 0; i < triangleMatrixSize-1; i += 2)
        {
            for (int j = 0; j < triangleMatrixSize-1; j += 2)
            {
                var connectionPoint = Instantiate(circlePrefab, new Vector3(i+1, j+1, 0), Quaternion.identity, connectionPoints.transform);
                var cell = Instantiate(gridCell, new Vector3(i+1, j+1, 0), Quaternion.identity, cellBackGrounds.transform);
            }
        }
    }
    
    
    void CreateTriangleGameObject(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        GameObject triangleObject = new GameObject("Triangle");
        triangleObject.transform.parent = level.transform;
        
        var triangle = triangleObject.AddComponent<Triangle>();

        triangle.Initialize(v1, v2, v3);
    }
    
    public void CenterTheCamera()
    {
        Triangle[] triangles = level.GetComponentsInChildren<Triangle>();
        
        Vector3 center = Vector3.zero;
        foreach (Triangle triangle in triangles)
        {
            center += triangle.transform.position;
        }
        center /= triangles.Length;
        
        Camera.main.transform.position = new Vector3(center.x, center.y -5f, -5f);

    }

    void MakeOutline(float gridSize)
    {
        GameObject Walls = new GameObject("Walls");
        Walls.transform.parent = level.transform;
        for (int x = -1; x <= gridSize*2+1; x++)
        {
            for (int y = -1; y <= gridSize*2+1; y++)
            {
                // If the current position is on the border of the grid
                if (x == -1 || y == -1 || x == gridSize*2+1 || y == gridSize*2+1)
                {
                    // Create a new GameObject at this position
                    GameObject borderObject = Instantiate(wallPrefab,Walls.transform);
                    if(x > 0 && y > 0)
                        borderObject.transform.position = new Vector3(x-borderObject.transform.localScale.x/2, y-borderObject.transform.localScale.y/2, 0);
                    else if(x < 0 && y > 0)
                        borderObject.transform.position = new Vector3(x+borderObject.transform.localScale.x/2, y-borderObject.transform.localScale.y/2, 0);
                    else if(x > 0 && y < 0)
                        borderObject.transform.position = new Vector3(x-borderObject.transform.localScale.x/2, y+borderObject.transform.localScale.y/2, 0);
                    else if(x < 0 && y < 0)
                        borderObject.transform.position = new Vector3(x+borderObject.transform.localScale.x/2, y+borderObject.transform.localScale.y/2, 0);
                    else if(x == 0 || y == 0)
                        Destroy(borderObject);
                }
            }
        }

    }
    
}
