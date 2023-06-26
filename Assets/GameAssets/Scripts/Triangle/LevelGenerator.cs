using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public PieceGenerator PieceGenerator;
    public GameObject circlePrefab;
    public GameObject gridCell;
    List<Triangle> triangles = new List<Triangle>();
    public float gridSize = 3; // The size of your grid (3 for a 3x3 grid, 4 for a 4x4 grid, etc.)
    public float cellSize = 1;
    private void Awake()
    {
        PieceGenerator = GetComponent<PieceGenerator>();
    }

    void Start()
    {
        GenerateTriangles();
        PieceGenerator.GeneratePieces();
        GenerateGridPoints();
        //MakeOutlier();
    }

    void GenerateTriangles()
    {
        for (int i = 0; i < 7; i += 2)
        {
            for (int j = 0; j < 7; j += 2)
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
        for (int i = 0; i <= 6; i += 2)
        {
            for (int j = 0; j <= 6; j += 2)
            {
                // Instantiate a circle at each grid point
                Instantiate(circlePrefab, new Vector3(i+1, j+1, 0), Quaternion.identity, this.transform);
                var cell = Instantiate(gridCell, new Vector3(i+1, j+1, 0), Quaternion.identity, this.transform);
            }
        }
    }
    
    
    void CreateTriangleGameObject(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Create a new GameObject for the triangle
        GameObject triangleObject = new GameObject("Triangle");
        triangleObject.transform.parent = this.transform;
        
        var triangle = triangleObject.AddComponent<Triangle>();

        triangle.Initialize(v1, v2, v3);
    }

    void MakeOutlier()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        // Calculate the size of the grid in world units
        float gridWorldSize = 8 * cellSize;

        // Set the positions of the line
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, new Vector3(gridWorldSize, 0, 0));
        lineRenderer.SetPosition(2, new Vector3(gridWorldSize, gridWorldSize, 0));
        lineRenderer.SetPosition(3, new Vector3(0, gridWorldSize, 0));
        lineRenderer.SetPosition(4, new Vector3(0, 0, 0));

    }
}
