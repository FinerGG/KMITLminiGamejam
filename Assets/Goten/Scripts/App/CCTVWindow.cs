using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MGJ
{
    public class CCTVWindow : ResizableWindow
    {
        [Header("CCTV Display")]
        [SerializeField] private RawImage cameraDisplay;
        [SerializeField] private TextMeshProUGUI cameraLabel;

        [Header("Camera Selector")]
        [SerializeField] private Button[] cameraButtons; // 4 ปุ่ม

        [Header("Enemy Detection")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private LayerMask enemyLayer;

        private Camera[] cameras;
        private RenderTexture[] renderTextures;
        private int currentCameraIndex = 0;
        private List<EnemyAI> detectedEnemies = new List<EnemyAI>();

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

            // Setup ปุ่มสำหรับกล้อง
            for (int i = 0; i < cameraButtons.Length && i < cameras.Length; i++)
            {
                int index = i; // Capture for closure
                cameraButtons[i].onClick.RemoveAllListeners();
                cameraButtons[i].onClick.AddListener(() => SwitchCamera(index));
            }

            // แสดงกล้องแรก
            SwitchCamera(0);
        }

        private void Update()
        {
            // เช็คว่ามี Enemy ในมุมมองหรือไม่
            CheckEnemiesInView();
        }

        public void SwitchCamera(int index)
        {
            if (index < 0 || index >= cameras.Length) return;

            currentCameraIndex = index;

            // ����¹ display
            cameraDisplay.texture = renderTextures[index];

            // �ѻവ label
            cameraLabel.text = $"Camera {index + 1}";

            // Highlight ����������͡
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

            // ปิด Enemy detection
            ClearEnemyDetection();

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

        #region Enemy Detection

        /// <summary>
        /// เช็คว่ามี Enemy ในมุมมองหรือไม่
        /// </summary>
        private void CheckEnemiesInView()
        {
            if (cameras == null || currentCameraIndex >= cameras.Length)
                return;

            Camera currentCam = cameras[currentCameraIndex];
            if (currentCam == null)
                return;

            // หา Enemy ทั้งหมดในรัศมี
            Collider[] colliders = Physics.OverlapSphere(
                currentCam.transform.position,
                detectionRange,
                enemyLayer
            );

            // ล้างรายการเดิม
            ClearEnemyDetection();

            foreach (var col in colliders)
            {
                EnemyAI enemy = col.GetComponent<EnemyAI>();
                if (enemy == null)
                    continue;

                // เช็คว่าอยู่ในมุมมองหรือไม่
                Vector3 dirToEnemy = enemy.transform.position - currentCam.transform.position;
                float angle = Vector3.Angle(currentCam.transform.forward, dirToEnemy);

                if (angle < detectionAngle * 0.5f)
                {
                    // อยู่ในมุมมอง → ตั้งค่า Enemy ว่ากำลังถูกมอง
                    enemy.SetBeingWatched(true);
                    detectedEnemies.Add(enemy);
                }
            }
        }

        private void ClearEnemyDetection()
        {
            foreach (var enemy in detectedEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetBeingWatched(false);
                }
            }

            detectedEnemies.Clear();
        }

        #endregion
    }
}
