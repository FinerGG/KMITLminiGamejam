using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    public class WindowManager : Singleton<WindowManager>
    {
        [Header("Window Management")]
        [SerializeField] private Transform windowContainer;

        private List<Window> openWindows = new List<Window>();
        private Window focusedWindow = null;

        public void RegisterWindow(Window window)
        {
            if (!openWindows.Contains(window))
            {
                openWindows.Add(window);
                SetFocusedWindow(window);
            }
        }

        public void UnregisterWindow(Window window)
        {
            if (openWindows.Contains(window))
            {
                openWindows.Remove(window);

                if (focusedWindow == window)
                {
                    focusedWindow = null;
                    if (openWindows.Count > 0)
                    {
                        SetFocusedWindow(openWindows[openWindows.Count - 1]);
                    }
                }
            }
        }

        public void SetFocusedWindow(Window window)
        {
            if (focusedWindow == window) return;

            if (focusedWindow != null)
            {
                // Notify previous focused window
                focusedWindow.SendMessage("OnLoseFocus", SendMessageOptions.DontRequireReceiver);
            }

            focusedWindow = window;

            if (focusedWindow != null)
            {
                focusedWindow.SendMessage("OnFocus", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void OpenWindow(Window window)
        {
            if (window == null) return;
            window.Open();
        }

        public Window OpenWindow(GameObject windowPrefab, Transform parent = null)
        {
            if (windowPrefab == null) return null;

            Transform spawnParent = parent != null ? parent : windowContainer;
            if (spawnParent == null) spawnParent = transform;

            GameObject windowObj = Instantiate(windowPrefab, spawnParent);
            Window window = windowObj.GetComponent<Window>();

            if (window != null)
            {
                RegisterWindow(window);
                window.Open();
            }

            return window;
        }

        public void CloseAllWindows()
        {
            // Create a copy to avoid modification during iteration
            List<Window> windowsCopy = new List<Window>(openWindows);
            foreach (Window window in windowsCopy)
            {
                window.Close();
            }
            openWindows.Clear();
            focusedWindow = null;
        }

        public void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        public List<Window> GetOpenWindows()
        {
            return new List<Window>(openWindows);
        }

        public Window GetFocusedWindow()
        {
            return focusedWindow;
        }

        public int GetOpenWindowCount()
        {
            return openWindows.Count;
        }
    }
}
