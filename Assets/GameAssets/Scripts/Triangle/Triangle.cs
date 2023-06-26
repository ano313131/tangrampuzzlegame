using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle: MonoBehaviour
{
    [SerializeField] public Material material;
    public List<Triangle> neighbors = new List<Triangle>();
    public Vector3[] vertices;
    
    public void Initialize(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        vertices = new Vector3[] { v1, v2, v3 };
        CreateMesh();
    }
    
    
    public void FindNeighbors()
    {
        // Get all the other triangles from the TriangleMatrix
        Triangle[] allTriangles = transform.parent.GetComponentsInChildren<Triangle>();

        // Find the neighboring triangles
        foreach (Triangle triangle in allTriangles)
        {
            if (triangle != this && AreNeighbors(this, triangle))
            {
                neighbors.Add(triangle);
            }
        }
    }
    
    static bool AreNeighbors(Triangle t1, Triangle t2)
    {
        // Two triangles are neighbors if they share at least two vertices
        int sharedVertices = 0;
        foreach (Vector3 vertex in t1.vertices)
        {
            if (System.Array.Exists(t2.vertices, v => v == vertex))
            {
                sharedVertices++;
            }
        }
        return sharedVertices >= 2;
    }

    public void CreateMesh()
    {
        // Calculate the center of the triangle
        Vector3 center = (vertices[0] + vertices[1] + vertices[2]) / 3;

        // Adjust the positions of the vertices so that the center of the triangle is at the origin
        Vector3[] adjustedVertices = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            adjustedVertices[i] = vertices[i] - center;
        }

        // Create the mesh
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = default;
        Mesh mesh = new Mesh();
        mesh.vertices = adjustedVertices;
        mesh.triangles = new int[] { 0, 1, 2 };
        meshFilter.mesh = mesh;
        

        // Adjust the position of the triangle's GameObject so that the visual center of the triangle aligns with its transform position
        transform.position = center;
    }
}
