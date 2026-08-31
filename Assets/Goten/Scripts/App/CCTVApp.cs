using UnityEngine;

namespace MGJ
{
    public class CCTVApp : App
    {
        [Header("CCTV Settings")]
        [SerializeField] private Camera[] cctvCameras; // กล้อง 4 ตัว

        private void Start()
        {
            foreach (var cam in cctvCameras) {
               cam.enabled = false;
            }
        }

        protected override void OnWindowOpened(Window window)
        {
            CCTVWindow cctvWindow = window as CCTVWindow;
            if (cctvWindow != null)
            {
                cctvWindow.Initialize(cctvCameras);
            }
        }
    }
}
