using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    /// <summary>
    /// จัดการการสลับ Environment/Layout แต่ละรอบเมื่อผู้เล่นตาย
    /// ทำให้แต่ละรอบมี layout ที่แตกต่างกัน
    /// </summary>
    public class EnvironmentManager : Singleton<EnvironmentManager>
    {
        [Header("Environment Rotation")]
        [SerializeField] private List<GameObject> environments = new List<GameObject>();
        [SerializeField] private int currentEnvironmentIndex = 0;
        [SerializeField] private bool randomOrder = false;

        private void Start()
        {
            // ตั้งค่าเริ่มต้น: เปิดเฉพาะ environment แรก
            InitializeEnvironments();
        }

        private void InitializeEnvironments()
        {
            if (environments.Count == 0)
            {
                Debug.LogWarning("[EnvironmentManager] ไม่มี Environment ใน List!");
                return;
            }

            // ปิดทุก environment
            for (int i = 0; i < environments.Count; i++)
            {
                if (environments[i] != null)
                {
                    environments[i].SetActive(i == currentEnvironmentIndex);
                }
            }

            Debug.Log($"[EnvironmentManager] เปิด Environment {currentEnvironmentIndex}: {environments[currentEnvironmentIndex].name}");
        }

        /// <summary>
        /// สลับไป Environment ถัดไป
        /// </summary>
        public void SwitchToNextEnvironment()
        {
            StartCoroutine(SwitchToNextEnvironmentCoroutine());
        }

        private System.Collections.IEnumerator SwitchToNextEnvironmentCoroutine()
        {
            if (environments.Count == 0)
            {
                Debug.LogWarning("[EnvironmentManager] ไม่มี Environment ให้สลับ!");
                yield break;
            }

            // ปิด environment ปัจจุบัน
            if (environments[currentEnvironmentIndex] != null)
            {
                environments[currentEnvironmentIndex].SetActive(false);
            }

            // รอ 1 frame ให้ Unity ทำการ cleanup
            yield return null;

            // คำนวณ index ถัดไป
            if (randomOrder)
            {
                // สุ่ม environment (ไม่เอาตัวเดิม)
                int newIndex = currentEnvironmentIndex;
                int attempts = 0;
                while (newIndex == currentEnvironmentIndex && attempts < 100)
                {
                    newIndex = Random.Range(0, environments.Count);
                    attempts++;
                }
                currentEnvironmentIndex = newIndex;
            }
            else
            {
                // เดินตามลำดับ
                if (currentEnvironmentIndex < environments.Count - 1)
                    currentEnvironmentIndex++;
            }

            // เปิด environment ใหม่
            if (environments[currentEnvironmentIndex] != null)
            {
                environments[currentEnvironmentIndex].SetActive(true);
                Debug.Log($"[EnvironmentManager] 🔄 สลับไป Environment {currentEnvironmentIndex}: {environments[currentEnvironmentIndex].name}");
            }

            // รอ 1 frame ให้ environment เริ่มต้น
            yield return null;
        }

        /// <summary>
        /// รีเซ็ตกลับไป Environment แรก
        /// </summary>
        public void ResetToFirstEnvironment()
        {
            currentEnvironmentIndex = 0;
            InitializeEnvironments();
        }

        /// <summary>
        /// ดึง Environment ปัจจุบัน
        /// </summary>
        public GameObject GetCurrentEnvironment()
        {
            if (currentEnvironmentIndex >= 0 && currentEnvironmentIndex < environments.Count)
            {
                return environments[currentEnvironmentIndex];
            }
            return null;
        }

        /// <summary>
        /// ดึง index ปัจจุบัน
        /// </summary>
        public int GetCurrentIndex()
        {
            return currentEnvironmentIndex;
        }
    }
}
