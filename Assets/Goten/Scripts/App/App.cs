using UnityEngine;

namespace MGJ
{
    public class App : MonoBehaviour
    {
        [Header("App Settings")]
        [SerializeField] protected string appName = "app";
        [SerializeField] protected GameObject windowPrefab;

        protected bool isOpen = false;
        protected Window currentWindow = null;

        public virtual void Open()
        {
            // If window exists (but closed), reopen it
            if (currentWindow != null)
            {
                if (!currentWindow.gameObject.activeSelf)
                {
                    currentWindow.gameObject.SetActive(true);
                    WindowManager.Instance.RegisterWindow(currentWindow);
                    currentWindow.Open();
                }
                else
                {
                    // Already open, just bring to front
                    currentWindow.BringToFront();
                }
                isOpen = true;
                return;
            }

            // No window exists, create new one
            isOpen = true;

            if (windowPrefab != null)
            {
                currentWindow = WindowManager.Instance.OpenWindow(windowPrefab);

                if (currentWindow != null)
                {
                    OnWindowOpened(currentWindow);
                }
            }
            else
            {
                Debug.LogWarning($"App '{appName}': windowPrefab is null!");
            }
        }

        public virtual void Close()
        {
            if (currentWindow != null)
            {
                WindowManager.Instance.CloseWindow(currentWindow);
                // Don't set to null - keep reference for reopening
                // currentWindow = null;
            }
            isOpen = false;
        }

        protected virtual void OnWindowOpened(Window window)
        {
            // Override in derived classes to setup window content
        }

        public string AppName => appName;
        public bool IsOpen => isOpen;
        public Window CurrentWindow => currentWindow;
    }
}