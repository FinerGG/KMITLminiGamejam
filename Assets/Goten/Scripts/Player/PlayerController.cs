using UnityEngine;

namespace MGJ
{
    public class PlayerController : Singleton<PlayerController>
    {
        [Header("Respawn Settings")]
        [SerializeField] private Transform spawnPoint;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Transform playerTransform;

        protected void Awake()
        {
            playerTransform = transform;

            // บันทึกตำแหน่งและการหมุนเริ่มต้น
            if (spawnPoint != null)
            {
                initialPosition = spawnPoint.position;
                initialRotation = spawnPoint.rotation;
            }
            else
            {
                initialPosition = playerTransform.position;
                initialRotation = playerTransform.rotation;
            }
        }

        void Start()
        {

        }

        void Update()
        {

        }

        /// <summary>
        /// รีเซ็ตตำแหน่งและการหมุนของผู้เล่นกลับไปจุดเริ่มต้น
        /// </summary>
        public void ResetToSpawnPoint()
        {
            if (playerTransform != null)
            {
                // รีเซ็ตตำแหน่ง
                playerTransform.position = initialPosition;
                playerTransform.rotation = initialRotation;

                // รีเซ็ตการหมุนของกล้อง (ถ้ามี CameraController)
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ResetRotation();
                }

                Debug.Log($"[PlayerController] รีเซ็ตผู้เล่นไปตำแหน่ง: {initialPosition}");
            }
        }

        /// <summary>
        /// ตั้งจุด spawn ใหม่
        /// </summary>
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            if (newSpawnPoint != null)
            {
                spawnPoint = newSpawnPoint;
                initialPosition = newSpawnPoint.position;
                initialRotation = newSpawnPoint.rotation;
            }
        }
    }

}