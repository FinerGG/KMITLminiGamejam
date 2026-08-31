using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MGJ
{
    public class CCTVWindow : ResizableWindow
    {
        [Header("CCTV Display")]
        [SerializeField] private RawImage cameraDisplay;
        [SerializeField] private TextMeshProUGUI cameraLabel;

        [Header("Camera Selector")]
        [SerializeField] private Button[] cameraButtons; // 4 ปุ่ม

        private Camera[] cameras;
        private RenderTexture[] renderTextures;
        private int currentCameraIndex = 0;

        public void Initialize(Camera[] cctvCameras)
        {
            cameras = cctvCameras;

            foreach (var cam in cameras)
            {
                cam.enabled = true;
            }

            // สร้าง RenderTexture สำหรับแต่ละกล้อง
            renderTextures = new RenderTexture[cameras.Length];
            for (int i = 0; i < cameras.Length; i++)
            {
                renderTextures[i] = new RenderTexture(1280, 720, 24);
                cameras[i].targetTexture = renderTextures[i];
            }

            // Setup ปุ่มสลับกล้อง
            for (int i = 0; i < cameraButtons.Length && i < cameras.Length; i++)
            {
                int index = i; // Capture for closure
                cameraButtons[i].onClick.RemoveAllListeners();
                cameraButtons[i].onClick.AddListener(() => SwitchCamera(index));
            }

            // แสดงกล้องแรก
            SwitchCamera(0);
        }

        public void SwitchCamera(int index)
        {
            if (index < 0 || index >= cameras.Length) return;

            currentCameraIndex = index;

            // เปลี่ยน display
            cameraDisplay.texture = renderTextures[index];

            // อัปเดต label
            cameraLabel.text = $"Camera {index + 1}";

            // Highlight ปุ่มที่เลือก
            for (int i = 0; i < cameraButtons.Length; i++)
            {
                ColorBlock colors = cameraButtons[i].colors;
                colors.normalColor = (i == index) ? Color.green : Color.white;
                cameraButtons[i].colors = colors;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            foreach (var cam in cameras)
            {
                cam.enabled = false;
            }

            // ทำความสะอาด RenderTexture
            if (renderTextures != null)
            {
                foreach (var rt in renderTextures)
                {
                    if (rt != null) rt.Release();
                }
            }
        }
    }
}
