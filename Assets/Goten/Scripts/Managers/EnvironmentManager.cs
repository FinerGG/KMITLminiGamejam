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
        [Header("Loading Settings")]
        [SerializeField] private int objectsPerFrame = 10; // จำนวน object ที่โหลดต่อ frame
        [SerializeField] private bool useProgressiveLoading = true;
        [SerializeField] private bool useBlinkEffect = true; // ใช้เอฟเฟกต์กระพริบตา
        [SerializeField] private float blinkDuration = 0.4f; // ระยะเวลากระพริบ
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

            // ใช้ Blink Effect ถ้าเปิดใช้งาน
            if (useBlinkEffect && BlinkEffect.Instance != null)
            {
                bool switchComplete = false;

                // เริ่มกระพริบตา และสลับ environment ตอนกลางการกระพริบ
                BlinkEffect.Instance.Blink(blinkDuration, () =>
                {
                    StartCoroutine(SwitchEnvironmentImmediate());
                    switchComplete = true;
                });

                // รอให้การสลับเสร็จ
                yield return new WaitUntil(() => switchComplete);
            }
            else
            {
                // สลับแบบปกติ (ไม่มี blink effect)
                yield return StartCoroutine(SwitchEnvironmentImmediate());
            }
        }

        /// <summary>
        /// สลับ Environment ทันที (ใช้ตอนกลางการกระพริบ)
        /// </summary>
        private System.Collections.IEnumerator SwitchEnvironmentImmediate()
        {
            // ปิด environment ปัจจุบัน
            if (environments[currentEnvironmentIndex] != null)
            {
                if (useProgressiveLoading)
                {
                    yield return StartCoroutine(DeactivateEnvironmentProgressive(environments[currentEnvironmentIndex]));
                }
                else
                {
                    environments[currentEnvironmentIndex].SetActive(false);
                }
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
                Debug.Log($"[EnvironmentManager] 🔄 กำลังโหลด Environment {currentEnvironmentIndex}: {environments[currentEnvironmentIndex].name}");

                if (useProgressiveLoading)
                {
                    yield return StartCoroutine(ActivateEnvironmentProgressive(environments[currentEnvironmentIndex]));
                }
                else
                {
                    environments[currentEnvironmentIndex].SetActive(true);
                }

                Debug.Log($"[EnvironmentManager] ✓ โหลดเสร็จสิ้น Environment {currentEnvironmentIndex}");
            }

            yield return null;
        }

        /// <summary>
        /// เปิด Environment แบบค่อยเป็นค่อยไป เพื่อลด lag spike
        /// </summary>
        private System.Collections.IEnumerator ActivateEnvironmentProgressive(GameObject environment)
        {
            // เปิด GameObject หลักก่อน (แต่ยังไม่เปิด children)
            environment.SetActive(true);
            yield return null;

            // ดึง children ทั้งหมด (level แรกเท่านั้น)
            Transform[] children = new Transform[environment.transform.childCount];
            for (int i = 0; i < environment.transform.childCount; i++)
            {
                children[i] = environment.transform.GetChild(i);
                children[i].gameObject.SetActive(false); // ปิดไว้ก่อน
            }

            // เปิดทีละหลายๆ object ต่อ frame
            int count = 0;
            foreach (Transform child in children)
            {
                if (child != null)
                {
                    child.gameObject.SetActive(true);
                    count++;

                    // ถ้าเปิดครบตามจำนวนที่กำหนด ให้รอ 1 frame
                    if (count >= objectsPerFrame)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            yield return null;
        }

        /// <summary>
        /// ปิด Environment แบบค่อยเป็นค่อยไป
        /// </summary>
        private System.Collections.IEnumerator DeactivateEnvironmentProgressive(GameObject environment)
        {
            // ปิด children ก่อน
            Transform[] children = new Transform[environment.transform.childCount];
            for (int i = 0; i < environment.transform.childCount; i++)
            {
                children[i] = environment.transform.GetChild(i);
            }

            int count = 0;
            foreach (Transform child in children)
            {
                if (child != null && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    count++;

                    if (count >= objectsPerFrame)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            // ปิด GameObject หลัก
            environment.SetActive(false);
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
