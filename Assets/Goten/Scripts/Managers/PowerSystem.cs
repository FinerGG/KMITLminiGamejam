using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    /// <summary>
    /// จัดการพลังงานในเกม
    /// ลดลงเมื่อล็อคประตู
    /// </summary>
    public class PowerSystem : Singleton<PowerSystem>
    {
        [Header("Power Settings")]
        [SerializeField] private float maxPower = 100f;
        [SerializeField] private float currentPower = 100f;
        [SerializeField] private float drainRatePerDoor = 1f; // พลังงานลดต่อวินาทีต่อประตู

        [Header("Events")]
        public UnityEvent<float> OnPowerChanged; // เรียกเมื่อพลังงานเปลี่ยน (ส่งค่า %)
        public UnityEvent OnPowerDepleted; // เรียกเมื่อพลังงานหมด

        private int activeLocksCount = 0; // จำนวนประตูที่ล็อคอยู่
        private bool powerDepleted = false;

        public float CurrentPower => currentPower;
        public float MaxPower => maxPower;
        public float PowerPercentage => (currentPower / maxPower) * 100f;
        public bool HasPower => currentPower > 0f;
        public int ActiveLocksCount => activeLocksCount;

        private void Start()
        {
            ResetPower();
        }

        private void Update()
        {
            if (activeLocksCount > 0 && HasPower)
            {
                DrainPower(drainRatePerDoor * activeLocksCount * Time.deltaTime);
            }
        }

        #region Power Management

        /// <summary>
        /// ลดพลังงาน
        /// </summary>
        public void DrainPower(float amount)
        {
            if (powerDepleted)
                return;

            currentPower -= amount;
            currentPower = Mathf.Max(0f, currentPower);

            OnPowerChanged?.Invoke(PowerPercentage);

            // เช็คว่าพลังงานหมดหรือยัง
            if (currentPower <= 0f && !powerDepleted)
            {
                powerDepleted = true;
                OnPowerDepleted?.Invoke();
                Debug.Log("[PowerSystem] ⚠ พลังงานหมด!");
            }
        }

        /// <summary>
        /// เพิ่มพลังงาน (สำหรับอนาคต)
        /// </summary>
        public void AddPower(float amount)
        {
            currentPower += amount;
            currentPower = Mathf.Min(currentPower, maxPower);
            OnPowerChanged?.Invoke(PowerPercentage);
        }

        /// <summary>
        /// รีเซ็ตพลังงานเป็น 100%
        /// </summary>
        public void ResetPower()
        {
            currentPower = maxPower;
            powerDepleted = false;
            activeLocksCount = 0;
            OnPowerChanged?.Invoke(PowerPercentage);
            Debug.Log("[PowerSystem] รีเซ็ตพลังงาน");
        }

        #endregion

        #region Door Lock Management

        /// <summary>
        /// เรียกเมื่อประตูถูกล็อค
        /// </summary>
        public void RegisterLock()
        {
            activeLocksCount++;
            Debug.Log($"[PowerSystem] ล็อคประตู: {activeLocksCount} ประตู");
        }

        /// <summary>
        /// เรียกเมื่อประตูถูกปลดล็อค
        /// </summary>
        public void UnregisterLock()
        {
            activeLocksCount--;
            activeLocksCount = Mathf.Max(0, activeLocksCount);
            Debug.Log($"[PowerSystem] ปลดล็อคประตู: {activeLocksCount} ประตู");
        }

        #endregion
    }
}
