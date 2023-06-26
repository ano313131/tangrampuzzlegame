using System.Collections.Generic;
using UnityEngine;

public class PieceGenerator : MonoBehaviour
{
    public GameObject connectionPointPrefab;
    public GameObject triangleMatrix; // Assign your TriangleMatrix in the inspector
    public int pieceCount = 3; // The desired number of pieces
    public int averagePieceSize = 12; // The average size of each piece
    public int pieceSizeVariation = 6; // The variation in the size of each piece
    public Color[] pieceColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan, Color.white };
    
    public void GeneratePieces()
    {
        // Get all the triangles from the TriangleMatrix
        List<Triangle> triangles = new List<Triangle>(triangleMatrix.GetComponentsInChildren<Triangle>());

        // Find the neighbors of each triangle
        foreach (Triangle triangle in triangles)
        {
            triangle.FindNeighbors();
        }

        // Generate the pieces
        for (int i = 0; i < pieceCount; i++)
        {
            // Calculate the size of the piece
            int pieceSize = averagePieceSize + Random.Range(-pieceSizeVariation, pieceSizeVariation);

            // Create a new GameObject for the piece
            GameObject pieceObject = new GameObject("Piece" + (i + 1));
            pieceObject.tag = "Piece";
            pieceObject.transform.parent = this.transform;

            // Start with a random triangle
            Triangle currentTriangle = triangles[Random.Range(0, triangles.Count)];

            // Add triangles to the piece
            for (int j = 0; j < pieceSize && triangles.Count > 0; j++)
            {
                // Remove the current triangle from the list
                triangles.Remove(currentTriangle);

                // Set the parent of the current triangle's GameObject to the piece's GameObject
                currentTriangle.transform.parent = pieceObject.transform;

                // Choose a random neighbor of the current triangle as the next triangle
                if (currentTriangle.neighbors.Count > 0)
                {
                    currentTriangle = currentTriangle.neighbors[Random.Range(0, currentTriangle.neighbors.Count)];
                }
                else if (triangles.Count > 0)
                {
                    // If the current triangle has no neighbors, choose a random triangle from the list
                    currentTriangle = triangles[Random.Range(0, triangles.Count)];
                }
            }
            
            Material pieceMaterial = new Material(Shader.Find("UI/Default"));
            pieceMaterial.color = pieceColors[i % pieceColors.Length];

            // Assign the Material to all the triangles in the piece
            foreach (MeshRenderer meshRenderer in pieceObject.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = pieceMaterial;
            }
        }
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        // Assign remaining triangles to pieces
        while (triangles.Count > 0)
        {
            // Get a list of all the pieces


            // Choose a random piece
            GameObject piece = pieces[Random.Range(0, pieces.Length)];

            // Choose a random triangle from the list
            Triangle triangle = triangles[Random.Range(0, triangles.Count)];

            // Check if the triangle has a neighbor in the piece
            foreach (Triangle neighbor in triangle.neighbors)
            {
                if (neighbor.transform.parent == piece.transform)
                {
                    // Remove the triangle from the list
                    triangles.Remove(triangle);

                    // Set the parent of the triangle's GameObject to the piece's GameObject
                    triangle.transform.parent = piece.transform;
                    triangle.GetComponent<MeshRenderer>().material = neighbor.GetComponent<MeshRenderer>().material;
                    break;
                }
            }
        }

        foreach (var piece in pieces)
        {
            AddConnectionPoints(piece);
        }
        

        for (int i = 0; i < pieceCount; i++)
        {
            Vector3 center = CalculateCenter(pieces[i]);

            // Adjust the positions of the triangles so that the center of the piece is at the origin
            foreach (Triangle triangle in pieces[i].GetComponentsInChildren<Triangle>())
            {
                triangle.transform.position -= center;
            }
            
            foreach (ConnectionPoint point in pieces[i].GetComponentsInChildren<ConnectionPoint>())
            {
                point.transform.position -= center;
            }
        }
        
        for (int i = 0; i < pieceCount; i++)
        {

            MeshFilter[] meshFilters = pieces[i].GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            for (int j = 0; j < meshFilters.Length; j++)
            {
                combine[j].mesh = meshFilters[j].sharedMesh;
                combine[j].transform = meshFilters[j].transform.localToWorldMatrix;
                //meshFilters[j].gameObject.SetActive(false);
            }
            Material pieceMaterial = new Material(Shader.Find("UI/Default"));
            pieceMaterial.color = pieceColors[i];
            
            pieces[i].AddComponent<MeshFilter>().mesh = new Mesh();
            pieces[i].GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            pieces[i].AddComponent<MeshRenderer>().material = pieceMaterial;
            pieces[i].gameObject.SetActive(true);
            pieces[i].AddComponent<Draggable>();
            PolygonCollider2D polygonCollider = pieces[i].AddComponent<PolygonCollider2D>();
            // Get all the vertices of the triangles in the piece
            var meshPoints = pieces[i].GetComponent<MeshFilter>().mesh.vertices;
            List<Vector2> vertices = new List<Vector2>();
            int counter = 0;
            polygonCollider.pathCount = meshPoints.Length / 3;
            for (var index = 0; index < meshPoints.Length; index+=3)
            {
                
                var point1 = meshPoints[index];
                var point2 = meshPoints[index+1];
                var point3 = meshPoints[index+2];
                vertices.Add(new Vector2(point1.x, point1.y));
                vertices.Add(new Vector2(point2.x, point2.y));
                vertices.Add(new Vector2(point3.x, point3.y));
                polygonCollider.SetPath(counter, vertices);
                vertices.Clear();
                counter++;
            }

            
            // Set the points of the polygon to the vertices of the triangles
            //polygonCollider.points = vertices.ToArray();
            var porintCount = polygonCollider.GetTotalPointCount();
            
            pieces[i].AddComponent<Rigidbody2D>();
            pieces[i].GetComponent<Rigidbody2D>().isKinematic = true;

        }
    }
    void AddConnectionPoints(GameObject piece)
    {
        // Get all the triangles in the piece
        Triangle[] triangles = piece.GetComponentsInChildren<Triangle>();

        // Create a list to store the connection points
        List<Vector3> connectionPoints = new List<Vector3>();

        // Add the vertices of each triangle to the list
        foreach (Triangle triangle in triangles)
        {
            foreach (Vector3 vertex in triangle.vertices)
            {
                // Only add the points that correspond to the original matrix
                if (vertex.x % 2 == 1 && vertex.y % 2 == 1 && !connectionPoints.Contains(vertex))
                {
                    connectionPoints.Add(vertex);
                }
            }
        }
        
        // Remove the connection points that are part of a complete square
        for (int i = connectionPoints.Count - 1; i >= 0; i--)
        {
            Vector3 point = connectionPoints[i];
            int count = 0;
            foreach (Triangle triangle in triangles)
            {
                if (System.Array.Exists(triangle.vertices, vertex => vertex == point))
                {
                    count++;
                }
            }
            if (count >= 4)
            {
                connectionPoints.RemoveAt(i);
            }
        }
        
        // Instantiate a connection point at each connection point
        foreach (Vector3 point in connectionPoints)
        {
            Instantiate(connectionPointPrefab, point, Quaternion.identity, piece.transform);
        }
        
    }
    Vector3 CalculateCenter(GameObject piece)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Triangle triangle in piece.GetComponentsInChildren<Triangle>())
        {
            foreach (Vector3 vertex in triangle.vertices)
            {
                sum += vertex;
                count++;
            }
        }

        return sum / count;
    }
}
