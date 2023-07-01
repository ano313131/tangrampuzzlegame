using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;
using Shapes2D;
using Unity.Mathematics;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 screenPoint;
    private Vector3 offset;
    private GameObject ghostObjPrefab;
    private GameObject ghostObj;
    private Vector3 connectionPos;
    private ConnectionPoint[] connectionPoints;
    public static Draggable selectedObject;
    private CompletionChecker completionChecker;
    private Piece piece;
    private void Start()
    {
        piece = GetComponent<Piece>();
        completionChecker = FindObjectOfType<CompletionChecker>();
        connectionPoints = FindObjectsOfType<ConnectionPoint>();
        connectionPoints = connectionPoints.Where(cp => !cp.transform.parent.CompareTag("Piece")).ToArray();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (selectedObject != null && selectedObject != this)
        {
            return;
        }
        
        selectedObject = this;
        CreateGhostObjPrefab();
        transform.gameObject.layer = LayerMask.NameToLayer("Default");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        MoveSelectedPieceToTop();
        foreach (ConnectionPoint connectionPoint in GetComponentsInChildren<ConnectionPoint>())
        {
            connectionPoint.transform.localScale = Vector3.zero;
            connectionPoint.transform.DOScale(.3f, 0.3f).SetEase(Ease.OutBack);
        }
        
    }

    private void CreateGhostObjPrefab()
    {
        ghostObj = new GameObject();
        ghostObj.transform.parent = gameObject.transform;
        PolygonCollider2D polygonCollider = ghostObj.AddComponent<PolygonCollider2D>();
        // Get all the vertices of the triangles in the piece
        var meshPoints = ghostObj.GetComponentInParent<MeshFilter>().mesh.vertices;
        //PolygonCollider2D polygonCollider = pieces[i].AddComponent<PolygonCollider2D>();
        int counter = 0;
        polygonCollider.pathCount = meshPoints.Length / 3;
        for (var index = 0; index < meshPoints.Length; index+=3)
        {
            List<Vector2> vertices = new List<Vector2>();
            // Define the shrink factor
            float shrinkFactor = 0.1f;

            // Get the points of the triangle
            Vector3 point1 = meshPoints[index];
            Vector3 point2 = meshPoints[index+1];
            Vector3 point3 = meshPoints[index+2];

            // Calculate the centroid of the triangle
            Vector3 centroid = (point1 + point2 + point3) / 3;

            // Move each point towards the centroid
            Vector3 shrunkPoint1 = point1 + (centroid - point1) * shrinkFactor;
            Vector3 shrunkPoint2 = point2 + (centroid - point2) * shrinkFactor;
            Vector3 shrunkPoint3 = point3 + (centroid - point3) * shrinkFactor;

            // Add the shrunk points to the vertices list
            vertices.Add(new Vector2(shrunkPoint1.x, shrunkPoint1.y));
            vertices.Add(new Vector2(shrunkPoint2.x, shrunkPoint2.y));
            vertices.Add(new Vector2(shrunkPoint3.x, shrunkPoint3.y));

            // Set the path of the polygon collider
            polygonCollider.SetPath(counter, vertices);
            vertices.Clear();
            counter++;
        }
    }

    void MoveSelectedPieceToTop()
    {
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (selectedObject != null && selectedObject != this)
        {
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Default");
        selectedObject = this;
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        
        float speed = 50f;
        transform.position = Vector3.LerpUnclamped(transform.position, curPosition, Time.deltaTime * speed);
        
        ConnectionPoint[] pieceConnectionPoints = GetComponentsInChildren<ConnectionPoint>();
        ConnectionPoint[] allConnectionPoints = FindObjectsOfType<ConnectionPoint>();
        allConnectionPoints = allConnectionPoints.Where(cp => !cp.transform.parent.CompareTag("Piece")).ToArray();

        Vector3 totalOffset = Vector3.zero;
        foreach (ConnectionPoint pieceConnectionPoint in pieceConnectionPoints)
        {
            ConnectionPoint nearestConnectionPoint = null;
            float minDistance = Mathf.Infinity;
            foreach (ConnectionPoint connectionPoint in allConnectionPoints)
            {
                float distance = Vector3.Distance(pieceConnectionPoint.transform.position, connectionPoint.transform.position);
                if (distance < minDistance)
                {
                    nearestConnectionPoint = connectionPoint;
                    minDistance = distance;
                }
            }

            // Calculate the offset needed to move the piece connection point to the nearest connection point
            if (nearestConnectionPoint != null)
            {
                totalOffset += nearestConnectionPoint.transform.position - pieceConnectionPoint.transform.position;
            }
        }

        var tempPos = transform.position;
        
        if (totalOffset.magnitude < 5f)
        {
            ghostObj.SetActive(true);
            var pos = transform.position + totalOffset / pieceConnectionPoints.Length;
            ghostObj.transform.position = pos;
            connectionPos = pos;
            SetPieceConnected();
        }
        else
        {
            connectionPos = tempPos;
            ghostObj.SetActive(false);
            SetPieceDisconnected();
        }
        
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (selectedObject != null && selectedObject != this)
        {
            return;
        }
        selectedObject = this;
        var ghostObjCollider = ghostObj.GetComponent<PolygonCollider2D>();
        bool isCollidingWall = ghostObjCollider.IsTouchingLayers(LayerMask.GetMask("Wall"));
        bool isCollidingPiece = ghostObjCollider.IsTouchingLayers(LayerMask.GetMask("Piece"));
        Debug.Log("iscollidingwall" + isCollidingWall);
        Debug.Log("iscollidingpiece" + isCollidingPiece);
        var tempPos = transform.position;



        if (isCollidingWall || isCollidingPiece)
        {
            transform.position = tempPos;
            SetPieceDisconnected();
        }
            
        else
        {
            transform.position = connectionPos;
        }
        
        foreach (ConnectionPoint connectionPoint in GetComponentsInChildren<ConnectionPoint>())
        {
            connectionPoint.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
        
        Destroy(ghostObj);
        transform.gameObject.layer = LayerMask.NameToLayer("Piece");
        selectedObject = null;
        completionChecker.CheckForLevelCompletion();
    }

    void SetPieceConnected()
    {
        piece.isConnected = true;
    }

    void SetPieceDisconnected()
    {
        piece.isConnected = false;
    }
}
