using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 screenPoint;
    private Vector3 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Find the nearest connection point for each connection point in the piece
        ConnectionPoint[] pieceConnectionPoints = GetComponentsInChildren<ConnectionPoint>();
        ConnectionPoint[] allConnectionPoints = FindObjectsOfType<ConnectionPoint>();

        // Filter out the connection points that belong to the piece being dragged
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

        if(totalOffset.magnitude < 5f)
            transform.position += totalOffset / pieceConnectionPoints.Length;
    }
}
