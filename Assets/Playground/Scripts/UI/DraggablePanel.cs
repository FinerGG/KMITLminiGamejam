using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;   // assign your main Canvas

    private RectTransform rt;
    private RectTransform canvasRt;
    private Vector2 pointerOffset;

    private void Awake()
    {
        rt = (RectTransform)transform;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        canvasRt = canvas.transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData e)
    {
        rt.SetAsLastSibling();

        // Pointer position in CANVAS local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, e.position, e.pressEventCamera, out var pointerCanvasLocal);

        // store offset between current anchoredPosition and pointer position
        pointerOffset = rt.anchoredPosition - pointerCanvasLocal;
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, e.position, e.pressEventCamera, out var pointerCanvasLocal);

        rt.anchoredPosition = pointerCanvasLocal + pointerOffset;
    }
}
