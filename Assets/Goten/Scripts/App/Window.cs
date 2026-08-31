using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MGJ
{
    public enum WindowState
    {
        Normal,
        Maximized,
        Minimized
    }

    public class Window : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [Header("Window Components")]
        [SerializeField] protected RectTransform windowRect;
        [SerializeField] protected RectTransform titleBar;
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button maximizeButton;
        [SerializeField] protected RectTransform resizeHandle;

        [Header("Window Settings")]
        [SerializeField] protected Vector2 minSize = new Vector2(200, 150);
        [SerializeField] protected Vector2 defaultSize = new Vector2(400, 300);
        [SerializeField] protected string windowTitle = "Window";
        [SerializeField] protected bool isResizable = true; // Enable/Disable resize

        protected WindowState currentState = WindowState.Normal;
        protected Vector2 normalPosition;
        protected Vector2 normalSize;
        protected Canvas parentCanvas;
        protected Vector2 dragOffset; // Track where user clicked on window

        protected virtual void Awake()
        {
            if (windowRect == null) windowRect = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();

            SetupButtons();
            ApplyResizableSettings();
        }

        protected virtual void ApplyResizableSettings()
        {
            // Hide maximize button if not resizable
            if (maximizeButton != null)
            {
                maximizeButton.gameObject.SetActive(isResizable);
            }
        }

        protected virtual void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }

            if (maximizeButton != null)
            {
                maximizeButton.onClick.RemoveAllListeners();
                maximizeButton.onClick.AddListener(ToggleMaximize);
            }
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);

            // Check if prefab is set to maximized (stretch anchors)
            if (windowRect.anchorMin == Vector2.zero && windowRect.anchorMax == Vector2.one)
            {
                currentState = WindowState.Maximized;
            }
            else
            {
                // Ensure anchors are set to center for normal state
                windowRect.anchorMin = new Vector2(0.5f, 0.5f);
                windowRect.anchorMax = new Vector2(0.5f, 0.5f);
                currentState = WindowState.Normal;
            }

            BringToFront();
            OnOpen();
        }

        public virtual void Close()
        {
            OnClose();
            WindowManager.Instance?.UnregisterWindow(this);
            gameObject.SetActive(false);
        }

        public virtual void Maximize()
        {
            if (currentState == WindowState.Maximized) return;

            // Store normal state
            normalPosition = windowRect.anchoredPosition;
            normalSize = windowRect.sizeDelta;

            // Stretch to fill parent by setting anchors
            windowRect.anchorMin = Vector2.zero;
            windowRect.anchorMax = Vector2.one;
            windowRect.anchoredPosition = Vector2.zero;
            windowRect.sizeDelta = Vector2.zero;

            currentState = WindowState.Maximized;
            OnMaximize();
        }

        public virtual void Restore()
        {
            if (currentState != WindowState.Maximized) return;

            // Restore anchors to center
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = normalPosition;
            windowRect.sizeDelta = normalSize;

            currentState = WindowState.Normal;
            OnRestore();
        }

        public virtual void ToggleMaximize()
        {
            if (currentState == WindowState.Maximized)
            {
                Restore();
            }
            else
            {
                Maximize();
            }
        }

        public virtual void BringToFront()
        {
            transform.SetAsLastSibling();
            WindowManager.Instance?.SetFocusedWindow(this);
        }

        // Drag window by title bar
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentState == WindowState.Maximized) return;
            BringToFront();

            // Calculate offset between mouse position and window position
            if (parentCanvas != null)
            {
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.GetComponent<RectTransform>(),
                    eventData.position,
                    parentCanvas.worldCamera,
                    out mousePos
                );
                dragOffset = mousePos - windowRect.anchoredPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentState == WindowState.Maximized) return;

            if (parentCanvas != null)
            {
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.GetComponent<RectTransform>(),
                    eventData.position,
                    parentCanvas.worldCamera,
                    out mousePos
                );

                // Calculate new position with drag offset
                Vector2 newPos = mousePos - dragOffset;

                // Constrain position to keep window within canvas bounds
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                Vector2 canvasSize = canvasRect.rect.size;
                Vector2 windowSize = windowRect.sizeDelta;
                Vector2 pivot = windowRect.pivot;

                // Calculate bounds based on pivot and size
                float halfWidth = windowSize.x * pivot.x;
                float halfHeight = windowSize.y * pivot.y;

                float minX = -canvasSize.x * 0.5f + halfWidth;
                float maxX = canvasSize.x * 0.5f - (windowSize.x - halfWidth);
                float minY = -canvasSize.y * 0.5f + halfHeight;
                float maxY = canvasSize.y * 0.5f - (windowSize.y - halfHeight);

                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

                // Apply constrained position
                windowRect.anchoredPosition = newPos;
            }
        }

        // Override these in derived classes
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnMaximize() { }
        protected virtual void OnRestore() { }
        protected virtual void OnFocus() { }
        protected virtual void OnLoseFocus() { }

        public WindowState State => currentState;
        public string Title => windowTitle;
    }
}
