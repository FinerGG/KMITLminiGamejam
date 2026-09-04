using UnityEngine;

namespace MGJ
{
    public class NetApp : App
    {
        [Header("Network Settings")]
        [SerializeField] private Canvas canvas;

        protected override void OnWindowOpened(Window window)
        {
            NetworkWindow netWindow = window as NetworkWindow;
            if (netWindow != null)
            {
                netWindow.SetCanvas(canvas);
            }
        }

    }

}