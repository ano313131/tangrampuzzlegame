using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
{
    private Vector3 screenPoint;
    private Vector3 offset;
    private GameObject ghostObj;
    private Vector3 connectionPos;
    private ConnectionPoint[] connectionPoints;
    public static Draggable selectedObject;
    private CompletionChecker completionChecker;
    private Piece piece;
    private PieceGenerator pieceGenerator;

    private void Start()
    {
        InitializeDraggable();
    }

    private void InitializeDraggable()
    {
        piece = GetComponent<Piece>();
        completionChecker = FindObjectOfType<CompletionChecker>();
        connectionPoints = FindObjectsOfType<ConnectionPoint>();
        pieceGenerator = FindObjectOfType<PieceGenerator>();
        connectionPoints = connectionPoints.Where(cp => !cp.transform.parent.CompareTag("Piece")).ToArray();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsCurrentObjectSelected()) return;
        
        HandleDragStart();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!IsCurrentObjectSelected()) return;

        HandleOnDrag();
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsCurrentObjectSelected()) return;
        
        HandleEndDrag();
    }



    private void HandleDragStart()
    {
        selectedObject = this;
        ghostObj = pieceGenerator.CreateGhostObjPrefab(gameObject);
        SetGameObjectLayerToDefault();
        CalculateScreenPointAndOffset();
        MoveSelectedPieceToTop();
        ScaleConnectionPoints();
    }
    
    private void HandleOnDrag()
    {
        SetGameObjectLayerToDefault();
        selectedObject = this;
        UpdatePosition();
        Vector3 totalOffset = CalculateTotalOffset();
        UpdateConnectionPos(totalOffset);
    }
    
    
    private void HandleEndDrag()
    {
        selectedObject = this;
        var isCollidingWall = IsGhostObjCollidingWithLayer("Wall");
        var isCollidingPiece = IsGhostObjCollidingWithLayer("Piece");
        var tempPos = transform.position;

        UpdateEndPosition(isCollidingWall, isCollidingPiece, tempPos);

        RescaleConnectionPoints();

        Destroy(ghostObj);
        SetGameObjectLayerToPiece();
        selectedObject = null;
        completionChecker.CheckForLevelCompletion();
    }
    

    private void SetGameObjectLayerToDefault()
    {
        transform.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void CalculateScreenPointAndOffset()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    private void MoveSelectedPieceToTop()
    {
        transform.SetAsLastSibling();
    }

    private void ScaleConnectionPoints()
    {
        foreach (ConnectionPoint connectionPoint in GetComponentsInChildren<ConnectionPoint>())
        {
            connectionPoint.transform.localScale = Vector3.zero;
            connectionPoint.transform.DOScale(.3f, 0.3f).SetEase(Ease.OutBack);
        }
    }
    

    private void UpdatePosition()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        
        float speed = 50f;
        transform.position = Vector3.LerpUnclamped(transform.position, curPosition, Time.deltaTime * speed);
    }

    private Vector3 CalculateTotalOffset()
    {
        ConnectionPoint[] pieceConnectionPoints = GetComponentsInChildren<ConnectionPoint>();
        ConnectionPoint[] allConnectionPoints = FindObjectsOfType<ConnectionPoint>();
        allConnectionPoints = allConnectionPoints.Where(cp => !cp.transform.parent.CompareTag("Piece")).ToArray();

        Vector3 totalOffset = Vector3.zero;
        foreach (ConnectionPoint pieceConnectionPoint in pieceConnectionPoints)
        {
            totalOffset += CalculateOffsetFromNearestConnectionPoint(pieceConnectionPoint, allConnectionPoints);
        }
        return totalOffset;
    }

    private Vector3 CalculateOffsetFromNearestConnectionPoint(ConnectionPoint pieceConnectionPoint, ConnectionPoint[] allConnectionPoints)
    {
        ConnectionPoint nearestConnectionPoint = GetNearestConnectionPoint(pieceConnectionPoint, allConnectionPoints);

        // Calculate the offset needed to move the piece connection point to the nearest connection point
        if (nearestConnectionPoint != null)
        {
            return nearestConnectionPoint.transform.position - pieceConnectionPoint.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private ConnectionPoint GetNearestConnectionPoint(ConnectionPoint pieceConnectionPoint, ConnectionPoint[] allConnectionPoints)
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
        return nearestConnectionPoint;
    }

    private void UpdateConnectionPos(Vector3 totalOffset)
    {
        ConnectionPoint[] pieceConnectionPoints = GetComponentsInChildren<ConnectionPoint>();
        var tempPos = transform.position;
        
        if (totalOffset.magnitude < 5f)
        {
            ActivateGhostObject();
            var pos = transform.position + totalOffset / pieceConnectionPoints.Length;
            ghostObj.transform.position = pos;
            connectionPos = pos;
            SetPieceConnected();
        }
        else
        {
            connectionPos = tempPos;
            DeactivateGhostObject();
            SetPieceDisconnected();
        }
    }

    private void ActivateGhostObject()
    {
        ghostObj.SetActive(true);
    }

    private void DeactivateGhostObject()
    {
        ghostObj.SetActive(false);
    }
    

    private bool IsGhostObjCollidingWithLayer(string layer)
    {
        var ghostObjCollider = ghostObj.GetComponent<PolygonCollider2D>();
        return ghostObjCollider.IsTouchingLayers(LayerMask.GetMask(layer));
    }

    private void UpdateEndPosition(bool isCollidingWall, bool isCollidingPiece, Vector3 tempPos)
    {
        if (isCollidingWall || isCollidingPiece)
        {
            transform.position = tempPos;
            SetPieceDisconnected();
        }
            
        else
        {
            transform.position = connectionPos;
        }
    }

    private void RescaleConnectionPoints()
    {
        foreach (ConnectionPoint connectionPoint in GetComponentsInChildren<ConnectionPoint>())
        {
            connectionPoint.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }

    private void SetGameObjectLayerToPiece()
    {
        transform.gameObject.layer = LayerMask.NameToLayer("Piece");
    }

    private void SetPieceConnected()
    {
        piece.isConnected = true;
    }

    private void SetPieceDisconnected()
    {
        piece.isConnected = false;
    }
    
    private bool IsCurrentObjectSelected()
    {
        return selectedObject == null || selectedObject == this;
    }
    
}
