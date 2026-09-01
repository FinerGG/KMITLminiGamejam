using UnityEngine;
using MGJ.Puzzle;

namespace MGJ
{
    /// <summary>
    /// ทำให้ Wire Puzzle สามารถ interact ได้ผ่านการคลิก
    /// สืบทอดจาก Interactable และใช้ CameraController ในการเปลี่ยนกล้อง
    /// </summary>
    public class WirePuzzleInteractable : Interactable
    {
        [Header("References")]
        [SerializeField] private WirePuzzleManager puzzleManager;
        [SerializeField] private Camera puzzleCamera;

        [Header("Interaction Settings")]
        [SerializeField] private string puzzleName = "Wire Connection Puzzle";
        [SerializeField] private KeyCode exitKey = KeyCode.Escape;

        [Header("Player Control")]
        [SerializeField] private bool disablePlayerMovement = true;
        [SerializeField] private bool disableCameraTurn = true;

        [Header("Camera Transition")]
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("UI")]
        [SerializeField] private GameObject puzzleUI;

        private bool isPlayerInteracting = false;

        private void Awake()
        {
            // Auto-find components
            if (puzzleManager == null)
                puzzleManager = GetComponentInChildren<WirePuzzleManager>();

            if (puzzleCamera == null)
                puzzleCamera = GetComponentInChildren<Camera>();

            // ปิด UI เริ่มต้น
            if (puzzleUI != null)
                puzzleUI.SetActive(false);

            // ปิดกล้อง Puzzle เริ่มต้น
            if (puzzleCamera != null)
                puzzleCamera.enabled = false;
        }

        private void Update()
        {
            // เช็คว่ามีคนคลิกหรือไม่
            if (interact && active)
            {
                interact = false; // Reset flag
                OnInteract();
            }

            // ออกจาก Puzzle
            if (isPlayerInteracting && Input.GetKeyDown(exitKey))
            {
                ExitPuzzle();
            }
        }

        /// <summary>
        /// เรียกเมื่อผู้เล่นคลิกที่วัตถุ (ถูกเรียกจาก Click() ใน base class)
        /// </summary>
        private void OnInteract()
        {
            if (!isPlayerInteracting)
            {
                StartPuzzle();
            }
            else
            {
                ExitPuzzle();
            }
        }

        /// <summary>
        /// เริ่ม Puzzle - เปลี่ยนกล้องและเปิดระบบ
        /// </summary>
        private void StartPuzzle()
        {
            if (puzzleCamera == null)
            {
                Debug.LogError($"[WirePuzzleInteractable] {gameObject.name}: ไม่มี puzzleCamera!");
                return;
            }

            if (CameraController.Instance == null)
            {
                Debug.LogError("[WirePuzzleInteractable] ไม่พบ CameraController.Instance!");
                return;
            }

            isPlayerInteracting = true;

            // เปลี่ยนกล้องไปยัง Puzzle Camera
            CameraController.Instance.SetCamera(puzzleCamera, transitionDuration);

            // ปิดการหมุนกล้อง
            if (disableCameraTurn)
            {
                CameraController.Instance.SetCameraTurnEnable(false);
            }

            // เปิด Puzzle Manager
            if (puzzleManager != null)
            {
                puzzleManager.ActivatePuzzle();
            }

            // แสดง UI
            if (puzzleUI != null)
            {
                puzzleUI.SetActive(true);
            }

            // ปิดการเคลื่อนที่ของผู้เล่น (ถ้าต้องการ)
            if (disablePlayerMovement && PlayerController.Instance != null)
            {
                // TODO: Disable player movement
                // PlayerController.Instance.SetMovementEnabled(false);
            }

            Debug.Log($"[WirePuzzleInteractable] เริ่ม Puzzle: {puzzleName}");
        }

        /// <summary>
        /// ออกจาก Puzzle - กลับสู่กล้องปกติ
        /// </summary>
        private void ExitPuzzle()
        {
            if (CameraController.Instance == null)
                return;

            isPlayerInteracting = false;

            // กลับสู่กล้องเดิม (Start Camera)
            CameraController.Instance.ReCamera();

            // ปิด Puzzle Manager
            if (puzzleManager != null)
            {
                puzzleManager.DeactivatePuzzle();
            }

            // ซ่อน UI
            if (puzzleUI != null)
            {
                puzzleUI.SetActive(false);
            }

            // เปิดการเคลื่อนที่ของผู้เล่น
            if (disablePlayerMovement && PlayerController.Instance != null)
            {
                // TODO: Enable player movement
                // PlayerController.Instance.SetMovementEnabled(true);
            }

            Debug.Log($"[WirePuzzleInteractable] ออกจาก Puzzle: {puzzleName}");
        }

        /// <summary>
        /// เรียกจาก Event เมื่อ Puzzle สำเร็จ
        /// </summary>
        public void OnPuzzleSolved()
        {
            Debug.Log($"[WirePuzzleInteractable] ✓ Puzzle สำเร็จ! {puzzleName}");

            // Auto exit หลังจาก 2 วินาที
            Invoke(nameof(ExitPuzzle), 2f);
        }

        /// <summary>
        /// เรียกจาก Event เมื่อมีการเชื่อมต่อสาย
        /// </summary>
        public void OnWireConnected()
        {
            // TODO: เล่น Sound Effect
        }

        /// <summary>
        /// เรียกจาก Event เมื่อปล่อยสาย
        /// </summary>
        public void OnWireDisconnected()
        {
            // TODO: เล่น Sound Effect
        }

        #region Public API

        public bool IsInteracting()
        {
            return isPlayerInteracting;
        }

        public string GetPuzzleName()
        {
            return puzzleName;
        }

        /// <summary>
        /// บังคับออกจาก Puzzle (เรียกจากภายนอก)
        /// </summary>
        public void ForceExit()
        {
            if (isPlayerInteracting)
            {
                ExitPuzzle();
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (puzzleCamera != null)
            {
                // แสดงตำแหน่งกล้อง Puzzle
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(puzzleCamera.transform.position, 0.2f);
                Gizmos.DrawLine(transform.position, puzzleCamera.transform.position);

                // แสดงทิศทางที่กล้องมอง
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(puzzleCamera.transform.position, puzzleCamera.transform.forward * 2f);
            }
        }

        #endregion
    }
}
