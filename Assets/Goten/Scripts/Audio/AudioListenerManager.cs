using UnityEngine;

namespace MGJ
{
    /// <summary>
    /// จัดการ Audio Listener ให้มีเพียงตัวเดียวในฉาก
    /// ติดตามกล้องที่ใช้งานอยู่จาก CameraController
    /// </summary>
    public class AudioListenerManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool followActiveCamera = true;
        [SerializeField] private bool removeOtherListenersOnStart = true;

        private AudioListener _audioListener;

        private void Awake()
        {
            // สร้าง Audio Listener บน GameObject นี้
            _audioListener = gameObject.GetComponent<AudioListener>();
            if (_audioListener == null)
            {
                _audioListener = gameObject.AddComponent<AudioListener>();
            }

            // ลบ Audio Listener อื่นๆ ในฉาก
            if (removeOtherListenersOnStart)
            {
                RemoveOtherAudioListeners();
            }
        }

        private void LateUpdate()
        {
            if (!followActiveCamera)
                return;

            // ติดตาม Camera ที่ใช้งานอยู่
            if (CameraController.Instance != null && CameraController.Instance.Camera != null)
            {
                Camera activeCamera = CameraController.Instance.Camera;

                // ย้าย Audio Listener ไปที่กล้องที่ใช้งาน
                if (transform.position != activeCamera.transform.position ||
                    transform.rotation != activeCamera.transform.rotation)
                {
                    transform.SetPositionAndRotation(
                        activeCamera.transform.position,
                        activeCamera.transform.rotation
                    );
                }
            }
        }

        /// <summary>
        /// ลบ Audio Listener อื่นๆ ที่ไม่ใช่ตัวนี้
        /// </summary>
        private void RemoveOtherAudioListeners()
        {
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            int removedCount = 0;

            foreach (var listener in allListeners)
            {
                if (listener != _audioListener)
                {
                    Debug.Log($"[AudioListenerManager] ลบ Audio Listener จาก: {listener.gameObject.name}");
                    Destroy(listener);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Debug.Log($"[AudioListenerManager] ลบ Audio Listener ทั้งหมด {removedCount} ตัว");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // แจ้งเตือนถ้ามี Audio Listener หลายตัว
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            if (allListeners.Length > 1)
            {
                Debug.LogWarning($"[AudioListenerManager] พบ Audio Listener {allListeners.Length} ตัวในฉาก! ควรมีเพียง 1 ตัว");
            }
        }
#endif
    }
}
