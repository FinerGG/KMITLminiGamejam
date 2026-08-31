using UnityEngine;
using UnityEngine.EventSystems;

namespace MGJ
{
    public class ResizableWindow : Window
    {
        [Header("Resize Settings")]
        [SerializeField] private RectTransform resizeHandleBottomRight;
        [SerializeField] private RectTransform resizeHandleBottomLeft;
        [SerializeField] private RectTransform resizeHandleTopRight;
        [SerializeField] private RectTransform resizeHandleTopLeft;
        [SerializeField] private float handleSize = 10f;

        private bool isResizing = false;
        private Vector2 resizeStartSize;
        private Vector2 resizeStartPos;
        private Vector2 resizeStartMousePos;
        private ResizeDirection resizeDirection;

        public enum ResizeDirection
        {
            BottomRight,
            BottomLeft,
            TopRight,
            TopLeft
        }

        protected override void Awake()
        {
            base.Awake();
            SetupResizeHandles();
        }

        protected override void ApplyResizableSettings()
        {
            base.ApplyResizableSettings();

            // Hide all resize handles if not resizable
            if (resizeHandleBottomRight != null)
                resizeHandleBottomRight.gameObject.SetActive(isResizable);
            if (resizeHandleBottomLeft != null)
                resizeHandleBottomLeft.gameObject.SetActive(isResizable);
            if (resizeHandleTopRight != null)
                resizeHandleTopRight.gameObject.SetActive(isResizable);
            if (resizeHandleTopLeft != null)
                resizeHandleTopLeft.gameObject.SetActive(isResizable);
        }

        private void SetupResizeHandles()
        {
            if (resizeHandleBottomRight != null)
            {
                AddResizeDrag(resizeHandleBottomRight, ResizeDirection.BottomRight);
            }
            if (resizeHandleBottomLeft != null)
            {
                AddResizeDrag(resizeHandleBottomLeft, ResizeDirection.BottomLeft);
            }
            if (resizeHandleTopRight != null)
            {
                AddResizeDrag(resizeHandleTopRight, ResizeDirection.TopRight);
            }
            if (resizeHandleTopLeft != null)
            {
                AddResizeDrag(resizeHandleTopLeft, ResizeDirection.TopLeft);
            }
        }

        private void AddResizeDrag(RectTransform handle, ResizeDirection direction)
        {
            ResizeHandle resizeHandle = handle.gameObject.GetComponent<ResizeHandle>();
            if (resizeHandle == null)
            {
                resizeHandle = handle.gameObject.AddComponent<ResizeHandle>();
            }
            resizeHandle.Initialize(this, direction);
        }

        public void StartResize(ResizeDirection direction, PointerEventData eventData)
        {
            // If maximized, restore to normal state first before resizing
            if (currentState == WindowState.Maximized ||
                (windowRect.anchorMin == Vector2.zero && windowRect.anchorMax == Vector2.one))
            {
                // Switch to normal mode with center anchors
                windowRect.anchorMin = new Vector2(0.5f, 0.5f);
                windowRect.anchorMax = new Vector2(0.5f, 0.5f);

                // Set a reasonable size to start resizing from
                Vector2 canvasSize = parentCanvas.GetComponent<RectTransform>().rect.size;
                windowRect.sizeDelta = new Vector2(canvasSize.x * 0.8f, canvasSize.y * 0.8f);
                windowRect.anchoredPosition = Vector2.zero;

                currentState = WindowState.Normal;
            }

            isResizing = true;
            resizeDirection = direction;
            resizeStartSize = windowRect.sizeDelta;
            resizeStartPos = windowRect.anchoredPosition;

            // Store mouse position at start
            if (parentCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.GetComponent<RectTransform>(),
                    eventData.position,
                    parentCanvas.worldCamera,
                    out resizeStartMousePos
                );
            }

            BringToFront();
        }

        public void DoResize(PointerEventData eventData)
        {
            if (!isResizing || currentState == WindowState.Maximized) return;

            if (parentCanvas == null) return;

            Vector2 currentMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(),
                eventData.position,
                parentCanvas.worldCamera,
                out currentMousePos
            );

            // Calculate mouse delta from start
            Vector2 mouseDelta = currentMousePos - resizeStartMousePos;

            // Get canvas actual size (not sizeDelta which might be wrong with anchors)
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.rect.size;

            // Calculate new size before clamping
            Vector2 newSize = resizeStartSize;

            switch (resizeDirection)
            {
                case ResizeDirection.BottomRight:
                    // Drag right = increase width, drag down = increase height
                    newSize.x = resizeStartSize.x + mouseDelta.x;
                    newSize.y = resizeStartSize.y + mouseDelta.y;
                    break;

                case ResizeDirection.BottomLeft:
                    // Drag left = decrease width, drag down = increase height
                    newSize.x = resizeStartSize.x - mouseDelta.x;
                    newSize.y = resizeStartSize.y + mouseDelta.y;
                    break;

                case ResizeDirection.TopRight:
                    // Drag right = increase width, drag up = decrease height
                    newSize.x = resizeStartSize.x + mouseDelta.x;
                    newSize.y = resizeStartSize.y - mouseDelta.y;
                    break;

                case ResizeDirection.TopLeft:
                    // Drag left = decrease width, drag up = decrease height
                    newSize.x = resizeStartSize.x - mouseDelta.x;
                    newSize.y = resizeStartSize.y - mouseDelta.y;
                    break;
            }

            // Clamp size to min and max
            newSize.x = Mathf.Clamp(newSize.x, minSize.x, canvasSize.x);
            newSize.y = Mathf.Clamp(newSize.y, minSize.y, canvasSize.y);

            // Calculate how much size actually changed (after clamping)
            Vector2 sizeDelta = newSize - resizeStartSize;

            // Calculate new position based on which corner is being dragged
            Vector2 newPos = resizeStartPos;
            Vector2 pivot = windowRect.pivot;

            switch (resizeDirection)
            {
                case ResizeDirection.BottomRight:
                    // Bottom-right stays fixed, top-left moves
                    // For pivot (0.5, 0.5): move by half the size change
                    newPos.x = resizeStartPos.x + sizeDelta.x * (0.5f - pivot.x);
                    newPos.y = resizeStartPos.y - sizeDelta.y * (0.5f - pivot.y);
                    break;

                case ResizeDirection.BottomLeft:
                    // Bottom-left stays fixed, top-right moves
                    newPos.x = resizeStartPos.x - sizeDelta.x * (0.5f + pivot.x);
                    newPos.y = resizeStartPos.y - sizeDelta.y * (0.5f - pivot.y);
                    break;

                case ResizeDirection.TopRight:
                    // Top-right stays fixed, bottom-left moves
                    newPos.x = resizeStartPos.x + sizeDelta.x * (0.5f - pivot.x);
                    newPos.y = resizeStartPos.y + sizeDelta.y * (0.5f + pivot.y);
                    break;

                case ResizeDirection.TopLeft:
                    // Top-left stays fixed, bottom-right moves
                    newPos.x = resizeStartPos.x - sizeDelta.x * (0.5f + pivot.x);
                    newPos.y = resizeStartPos.y + sizeDelta.y * (0.5f + pivot.y);
                    break;
            }

            // Constrain position so window stays within canvas bounds
            float halfWidth = newSize.x * pivot.x;
            float halfHeight = newSize.y * pivot.y;

            // Calculate min/max position based on pivot and size
            float minX = -canvasSize.x * 0.5f + halfWidth;
            float maxX = canvasSize.x * 0.5f - (newSize.x - halfWidth);
            float minY = -canvasSize.y * 0.5f + halfHeight;
            float maxY = canvasSize.y * 0.5f - (newSize.y - halfHeight);

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            windowRect.sizeDelta = newSize;
            windowRect.anchoredPosition = newPos;
        }

        public void EndResize()
        {
            isResizing = false;

            // Check if resized to full canvas size -> auto maximize
            if (currentState == WindowState.Normal && parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                Vector2 canvasSize = canvasRect.rect.size;

                // If window size matches canvas size (with small tolerance), maximize it
                float tolerance = 5f;
                if (Mathf.Abs(windowRect.sizeDelta.x - canvasSize.x) < tolerance &&
                    Mathf.Abs(windowRect.sizeDelta.y - canvasSize.y) < tolerance)
                {
                    Maximize();
                }
            }
        }

        // Inner class for resize handle
        private class ResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            private ResizableWindow window;
            private ResizeDirection direction;

            public void Initialize(ResizableWindow win, ResizeDirection dir)
            {
                window = win;
                direction = dir;
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (window != null)
                {
                    window.StartResize(direction, eventData);
                }
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (window != null)
                {
                    window.DoResize(eventData);
                }
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (window != null)
                {
                    window.EndResize();
                }
            }
        }
    }
}
