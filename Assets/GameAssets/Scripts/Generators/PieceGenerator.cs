using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PieceGenerator : MonoBehaviour
{
    public GameObject connectionPointPrefab;
    private GameObject levelGameObject; // Assign your TriangleMatrix in the inspector
    //public int pieceCount = 3; // The desired number of pieces
    private int averagePieceSize; // The average size of each piece
    private int pieceSizeVariation; // The variation in the size of each piece
    public Color[] pieceColors;

    private List<Triangle> triangles = new List<Triangle>();
    
        public void GeneratePieces(int pieceCount)
    {
        InitializePieceGenerator(pieceCount);
        GeneratePieceObjects(pieceCount);
        AssignRemainingTrianglesToPieces();
        AddConnectionPointsToPieces();
        AdjustPiecePositions();
        CombineMeshesAndAddComponents();
        DestroyTriangles();
        PositionPiecesOutsideGrid(pieceCount);
    }

    private void InitializePieceGenerator(int pieceCount)
    {
        levelGameObject = GameObject.FindGameObjectWithTag("Level");
        if (levelGameObject == null)
        {
            Debug.Log("Level GameObject is not found.");
        }
        else
        {
            Debug.Log("Level GameObject is found.");
            triangles = levelGameObject.GetComponentsInChildren<Triangle>().ToList();
            Debug.Log("Number of triangles: " + triangles.Count);
        }
        averagePieceSize = triangles.Count / pieceCount;
        pieceSizeVariation = (int)(averagePieceSize * 0.2);
        foreach (Triangle triangle in triangles)
        {
            Debug.Log("sa");
            triangle.FindNeighbors();
        }
    }

    private void GeneratePieceObjects(int pieceCount)
    {
        for (int i = 0; i < pieceCount; i++)
        {
            int pieceSize = averagePieceSize + Random.Range(-pieceSizeVariation, pieceSizeVariation);
            GameObject pieceObject = CreatePieceObject(i);
            pieceObject.AddComponent<Piece>().isConnected = false;
            AssignTrianglesToPiece(pieceSize, pieceObject);
            AssignMaterialToPiece(i, pieceObject);
            
        }
    }

    private GameObject CreatePieceObject(int pieceIndex)
    {
        GameObject pieceObject = new GameObject("Piece" + (pieceIndex + 1));
        pieceObject.tag = "Piece";
        pieceObject.layer = LayerMask.NameToLayer("Piece");
        pieceObject.transform.parent = levelGameObject.transform;
        return pieceObject;
    }

    private void AssignTrianglesToPiece(int pieceSize, GameObject pieceObject)
    {
        Triangle currentTriangle = triangles[Random.Range(0, triangles.Count)];
        for (int j = 0; j < pieceSize && triangles.Count > 0; j++)
        {
            triangles.Remove(currentTriangle);
            currentTriangle.transform.parent = pieceObject.transform;
            if (currentTriangle.neighbors.Count > 0)
            {
                currentTriangle = currentTriangle.neighbors[Random.Range(0, currentTriangle.neighbors.Count)];
            }
        }
    }

    private void AssignMaterialToPiece(int pieceIndex, GameObject pieceObject)
    {
        Material pieceMaterial = new Material(Shader.Find("UI/Default"));
        pieceMaterial.color = pieceColors[pieceIndex % pieceColors.Length];
        foreach (MeshRenderer meshRenderer in pieceObject.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material = pieceMaterial;
        }
    }

    private void AssignRemainingTrianglesToPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        while (triangles.Count > 0)
        {
            Triangle triangle = triangles[Random.Range(0, triangles.Count)];
            var neighbor = triangle.neighbors[Random.Range(0, triangle.neighbors.Count)];
            if (neighbor.transform.parent.gameObject.tag == "Piece")
            {
                triangle.transform.parent = neighbor.transform.parent;
                triangle.GetComponent<MeshRenderer>().material = neighbor.GetComponent<MeshRenderer>().material;
                triangles.Remove(triangle);
            }
        }
    }

    private void AddConnectionPointsToPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (var piece in pieces)
        {
            AddConnectionPoints(piece);
        }
    }

    private void AdjustPiecePositions()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        for (int i = 0; i < pieces.Length; i++)
        {
            Vector3 center = CalculateCenter(pieces[i]);
            foreach (Triangle triangle in pieces[i].GetComponentsInChildren<Triangle>())
            {
                triangle.transform.position -= center;
            }
            foreach (ConnectionPoint point in pieces[i].GetComponentsInChildren<ConnectionPoint>())
            {
                point.transform.position -= center;
            }
        }
    }

    private void CombineMeshesAndAddComponents()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        for (int i = 0; i < pieces.Length; i++)
        {
            CombineMeshes(pieces[i]);
            AddComponentsToPiece(pieces[i], i);
        }
    }

    private void CombineMeshes(GameObject piece)
    {
        MeshFilter[] meshFilters = piece.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int j = 0; j < meshFilters.Length; j++)
        {
            combine[j].mesh = meshFilters[j].sharedMesh;
            combine[j].transform = meshFilters[j].transform.localToWorldMatrix;
        }
        piece.AddComponent<MeshFilter>().mesh = new Mesh();
        piece.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    private void AddComponentsToPiece(GameObject piece, int i)
    {
        Material pieceMaterial = new Material(Shader.Find("UI/Default"));
        pieceMaterial.color = pieceColors[i];
        piece.AddComponent<MeshRenderer>().material = pieceMaterial;
        piece.gameObject.SetActive(true);
        piece.AddComponent<Draggable>();
        AddPolygonColliderToPiece(piece);
    }

    private void AddPolygonColliderToPiece(GameObject piece)
    {
        PolygonCollider2D polygonCollider = piece.AddComponent<PolygonCollider2D>();
        var meshPoints = piece.GetComponent<MeshFilter>().mesh.vertices;
        List<Vector2> vertices = new List<Vector2>();
        int counter = 0;
        polygonCollider.pathCount = meshPoints.Length / 3;
        for (var index = 0; index < meshPoints.Length; index += 3)
        {
            var point1 = meshPoints[index];
            var point2 = meshPoints[index + 1];
            var point3 = meshPoints[index + 2];
            vertices.Add(new Vector2(point1.x, point1.y));
            vertices.Add(new Vector2(point2.x, point2.y));
            vertices.Add(new Vector2(point3.x, point3.y));
            polygonCollider.SetPath(counter, vertices);
            vertices.Clear();
            counter++;
        }

        polygonCollider.isTrigger = true;
        var rb = piece.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    void AddConnectionPoints(GameObject piece)
    {
        List<Vector3> deletedConnectionPoints = new List<Vector3>();
        Triangle[] triangles = piece.GetComponentsInChildren<Triangle>();
        List<Vector3> connectionPoints = GetConnectionPoints(triangles);

        RemoveCompleteSquarePoints(connectionPoints, triangles, deletedConnectionPoints);
        InstantiateConnectionPoints(connectionPoints, piece);
        InstantiateDeletedConnectionPoints(deletedConnectionPoints, piece);

        deletedConnectionPoints.Clear();
    }

    List<Vector3> GetConnectionPoints(Triangle[] triangles)
    {
        List<Vector3> connectionPoints = new List<Vector3>();
        foreach (Triangle triangle in triangles)
        {
            foreach (Vector3 vertex in triangle.vertices)
            {
                if (vertex.x % 2 == 1 && vertex.y % 2 == 1 && !connectionPoints.Contains(vertex))
                {
                    connectionPoints.Add(vertex);
                }
            }
        }
        return connectionPoints;
    }

    void RemoveCompleteSquarePoints(List<Vector3> connectionPoints, Triangle[] triangles, List<Vector3> deletedConnectionPoints)
    {
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
                var item = connectionPoints[i];
                connectionPoints.RemoveAt(i);
                deletedConnectionPoints.Add(item);
            }
        }
    }

    void InstantiateConnectionPoints(List<Vector3> connectionPoints, GameObject piece)
    {
        foreach (Vector3 point in connectionPoints)
        {
            var item = Instantiate(connectionPointPrefab, point, Quaternion.identity, piece.transform);
            item.transform.localScale = Vector3.zero;
            item.GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    void InstantiateDeletedConnectionPoints(List<Vector3> deletedConnectionPoints, GameObject piece)
    {
        foreach (var point in deletedConnectionPoints)
        {
            if (!piece.GetComponentInChildren<ConnectionPoint>())
            {
                var item = Instantiate(connectionPointPrefab, point, Quaternion.identity, piece.transform);
                item.transform.localScale = Vector3.zero;
                if (item.GetComponent<Material>())
                {
                    item.GetComponent<Material>().color = Color.gray;
                }
            }
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
    
    private void PositionPiecesOutsideGrid(int pieceCount)
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        float screenWidth = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        float screenHeight = Camera.main.orthographicSize * 2.0f;
        float offset = 5f; // Distance from the grid

        foreach (GameObject piece in pieces)
        {
            Vector3 position;
            do
            {
                // Calculate a random position on the screen
                float xPos = Random.Range(-screenWidth / 4, screenWidth / 4);
                float yPos = Random.Range(-screenHeight / 4, screenHeight / 4);
                position = new Vector3(xPos, yPos, 0);
            }
            while (IsPositionOccupied(position, piece));

            // Set the position of the piece
            piece.transform.position = position;
        }
    }

    private bool IsPositionOccupied(Vector3 position, GameObject piece)
    {
        // Check if the position is within the grid
        float gridWidth = CalculateGridWidth();
        if (Mathf.Abs(position.x) < gridWidth / 2 && Mathf.Abs(position.y) < gridWidth / 2)
        {
            return true;
        }

        // Check if the position is occupied by another piece
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 5f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != piece)
            {
                return true;
            }
        }

        return false;
    }

    private float CalculateGridWidth()
    {
        // Assuming the grid is a square and its size is defined by the scale of the levelGameObject
        return levelGameObject.transform.localScale.x;
    }


    void DestroyTriangles()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (var piece in pieces)
        {
            foreach (var triangle in piece.GetComponentsInChildren<Triangle>())
            {
                Destroy(triangle.gameObject);
            }
        }
    }
}
