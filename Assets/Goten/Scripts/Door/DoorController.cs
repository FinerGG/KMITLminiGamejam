using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    /// <summary>
    /// ควบคุมประตูที่ผู้เล่นสามารถล็อคได้
    /// เมื่อคลิกประตู → กล้องเคลื่อนที่ไปจุดประตู
    /// ที่ประตู → Free Camera (Mouse Look)
    /// คลิก LockObject → ล็อคประตู (ใช้พลังงาน)
    /// </summary>
    public class DoorController : Interactable
    {
        [Header("References")]
        [SerializeField] private Camera doorCamera; // กล้องที่ประตู
        [SerializeField] private Transform doorMesh; // Mesh ของประตู (สำหรับ animation)
        [SerializeField] private GameObject lockObject; // GameObject ที่คลิกเพื่อล็อค
        [SerializeField] private AudioSource doorAudioSource;

        [Header("Door Settings")]
        [SerializeField] private float cameraTransitionDuration = 1f; // เวลาในการเคลื่อนกล้อง
        [SerializeField] private float lookYawLimit = 60f; // จำกัดการหันซ้าย-ขวาที่ประตู (0 = หมุนได้รอบตัว)
        [SerializeField] private float closeDuration = 1f; // เวลาในการปิดประตู
        [SerializeField] private Vector3 doorOpenRotation = Vector3.zero; // มุมเปิด
        [SerializeField] private Vector3 doorCloseRotation = new Vector3(0, -90, 0); // มุมปิด

        [Header("Lock Settings")]
        [SerializeField] private bool canLockWhenNoPower = false; // ล็อคได้เมื่อไม่มีไฟหรือไม่

        [Header("Audio")]
        [SerializeField] private AudioClip doorCloseSound;
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private AudioClip lockSound;

        [Header("State")]
        [SerializeField] private bool isPlayerAtDoor = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private bool isDoorClosed = false;

        [Header("Events")]
        public UnityEvent OnPlayerEnterDoor;
        public UnityEvent OnPlayerExitDoor;
        public UnityEvent OnDoorLocked;
        public UnityEvent OnDoorUnlocked;

        private Quaternion targetRotation;
        private Quaternion animFromRotation;
        private float animProgress = 1f;
        private bool isAnimating = false;

        public bool IsPlayerAtDoor => isPlayerAtDoor;
        public bool IsLocked => isLocked;
        public bool IsDoorClosed => isDoorClosed;

        private void Awake()
        {
            // ปิดกล้องประตูเริ่มต้น
            if (doorCamera != null)
                doorCamera.enabled = false;

            // ซ่อน LockObject เริ่มต้น
            if (lockObject != null)
                lockObject.SetActive(false);

            // ประตูเริ่มที่ท่าเปิดค้างไว้
            isLocked = false;
            isDoorClosed = false;
            targetRotation = Quaternion.Euler(doorOpenRotation);

            if (doorMesh != null)
                doorMesh.localRotation = targetRotation;
        }

        private void Update()
        {
            // เช็คว่ามีคนคลิกหรือไม่ (จาก Interactable)
            if (interact && active)
            {
                interact = false;
                OnDoorInteract();
            }

            // เช็คว่าผู้เล่นอยู่ที่ประตู และกด ESC
            if (isPlayerAtDoor && Input.GetKeyDown(KeyCode.Escape))
            {
                ExitDoor();
            }

            // Animate ประตู
            if (isAnimating)
            {
                animProgress += Time.deltaTime / Mathf.Max(0.01f, closeDuration);

                float t = Mathf.Clamp01(animProgress);
                t = t * t * (3f - 2f * t); // ease in-out ให้ดูเหมือนประตูจริง

                doorMesh.localRotation = Quaternion.Slerp(animFromRotation, targetRotation, t);

                if (animProgress >= 1f)
                {
                    doorMesh.localRotation = targetRotation;
                    isAnimating = false;
                }
            }
        }

        #region Door Interaction

        /// <summary>
        /// เรียกเมื่อผู้เล่นคลิกที่ประตู
        /// </summary>
        private void OnDoorInteract()
        {
            if (isPlayerAtDoor)
            {
                // ถ้าอยู่ที่ประตูอยู่แล้ว ให้ออก
                //ExitDoor();
            }
            else
            {
                // ถ้ายังไม่อยู่ที่ประตู ให้เข้าไป
                EnterDoor();
            }
        }

        /// <summary>
        /// ผู้เล่นเข้าไปที่ประตู
        /// </summary>
        private void EnterDoor()
        {
            if (doorCamera == null)
            {
                Debug.LogError($"[DoorController] {gameObject.name}: ไม่มี doorCamera!");
                return;
            }

            isPlayerAtDoor = true;

            // เปลี่ยนกล้องไปที่ประตู
            CameraController.Instance.SetCamera(doorCamera, cameraTransitionDuration);
            CameraController.Instance.SetCameraTurnEnable(true, lookYawLimit); // เปิด Free Camera

            // แสดง LockObject
            if (lockObject != null)
                lockObject.SetActive(true);

            OnPlayerEnterDoor?.Invoke();
            Debug.Log($"[DoorController] ผู้เล่นเข้าไปที่ประตู: {gameObject.name}");
        }

        /// <summary>
        /// ผู้เล่นออกจากประตู
        /// </summary>
        private void ExitDoor()
        {
            isPlayerAtDoor = false;

            // กลับไปกล้องเดิม
            CameraController.Instance.ReCamera();

            // ซ่อน LockObject
            if (lockObject != null)
                lockObject.SetActive(false);

            OnPlayerExitDoor?.Invoke();
            Debug.Log($"[DoorController] ผู้เล่นออกจากประตู: {gameObject.name}");
        }

        #endregion

        #region Lock/Unlock

        /// <summary>
        /// สลับสถานะล็อค (เรียกจาก DoorLockButton บน LockObject)
        /// ล็อค → ประตูค่อยๆปิด, ปลดล็อค → ประตูค่อยๆเปิด
        /// </summary>
        public void ToggleLock()
        {
            if (isLocked)
                UnlockDoor();
            else
                LockDoor();
        }

        /// <summary>
        /// ล็อคประตู (เรียกจาก LockObject)
        /// </summary>
        public void LockDoor()
        {
            if (isLocked)
            {
                Debug.Log($"[DoorController] ประตูล็อคอยู่แล้ว: {gameObject.name}");
                return;
            }

            // เช็คว่ามีพลังงานหรือไม่
            if (!canLockWhenNoPower && !PowerSystem.Instance.HasPower)
            {
                Debug.Log($"[DoorController] ไม่มีพลังงาน! ไม่สามารถล็อคได้: {gameObject.name}");
                return;
            }

            isLocked = true;

            // ปิดประตู
            CloseDoor();

            // ลงทะเบียนกับ PowerSystem
            PowerSystem.Instance.RegisterLock();

            // เล่นเสียง
            PlaySound(lockSound);

            OnDoorLocked?.Invoke();
            Debug.Log($"[DoorController] ล็อคประตู: {gameObject.name}");
        }

        /// <summary>
        /// ปลดล็อคประตู
        /// </summary>
        public void UnlockDoor()
        {
            if (!isLocked)
                return;

            isLocked = false;

            // เปิดประตู
            OpenDoor();

            // ยกเลิกการลงทะเบียนกับ PowerSystem
            PowerSystem.Instance.UnregisterLock();

            OnDoorUnlocked?.Invoke();
            Debug.Log($"[DoorController] ปลดล็อคประตู: {gameObject.name}");
        }

        #endregion

        #region Door Animation

        private void CloseDoor()
        {
            if (isDoorClosed)
                return;

            isDoorClosed = true;
            StartDoorAnimation(Quaternion.Euler(doorCloseRotation));

            PlaySound(doorCloseSound);
        }

        private void OpenDoor()
        {
            if (!isDoorClosed)
                return;

            isDoorClosed = false;
            StartDoorAnimation(Quaternion.Euler(doorOpenRotation));

            PlaySound(doorOpenSound);
        }

        private void StartDoorAnimation(Quaternion target)
        {
            animFromRotation = doorMesh != null ? doorMesh.localRotation : target;
            targetRotation = target;
            animProgress = 0f;
            isAnimating = true;
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip)
        {
            if (doorAudioSource != null && clip != null)
            {
                doorAudioSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Reset

        public void ResetDoor()
        {
            isLocked = false;
            isDoorClosed = false;
            isPlayerAtDoor = false;
            isAnimating = false;
            animProgress = 1f;

            targetRotation = Quaternion.Euler(doorOpenRotation);
            if (doorMesh != null)
                doorMesh.localRotation = targetRotation;

            if (lockObject != null)
                lockObject.SetActive(false);

            Debug.Log($"[DoorController] รีเซ็ตประตู: {gameObject.name}");
        }

        #endregion
    }
}
