using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    /// <summary>
    /// จัดการเวลาในเกม (00:00-05:00)
    /// 1 วินาทีในชีวิตจริง = X วินาทีในเกม
    /// </summary>
    public class TimeManager : Singleton<TimeManager>
    {
        [Header("Time Settings")]
        [SerializeField] private float realSecondsPerGameMinute = 5f; // 1 นาทีในเกม = 5 วินาทีจริง
        [SerializeField] private int startHour = 0; // เวลาเริ่มต้น 00:00
        [SerializeField] private int endHour = 5; // เวลาจบ 05:00

        [Header("Current Time")]
        [SerializeField] private int currentHour = 0;
        [SerializeField] private int currentMinute = 0;

        [Header("Events")]
        public UnityEvent<int> OnHourChanged; // เรียกเมื่อชั่วโมงเปลี่ยน
        public UnityEvent OnGameEnd; // เรียกเมื่อถึง 05:00

        private float gameTimer = 0f;
        private bool isRunning = false;
        private bool gameEnded = false;

        public int CurrentHour => currentHour;
        public int CurrentMinute => currentMinute;
        public float RealSecondsPerGameMinute => realSecondsPerGameMinute;
        public bool IsRunning => isRunning;
        public bool GameEnded => gameEnded;

        private void Start()
        {
            ResetTime();
        }

        private void Update()
        {
            if (!isRunning || gameEnded)
                return;

            UpdateGameTime();
        }

        private void UpdateGameTime()
        {
            gameTimer += Time.deltaTime;

            // เมื่อครบ 1 นาทีในเกม
            if (gameTimer >= realSecondsPerGameMinute)
            {
                gameTimer -= realSecondsPerGameMinute;
                currentMinute++;

                if (currentHour == 0 && currentMinute >= 30)
                {
                    EnemyAI.Instance.SetActive(true);
                }

                // ครบ 60 นาที = 1 ชั่วโมง
                if (currentMinute >= 60)
                {
                    currentMinute = 0;
                    currentHour++;

                    OnHourChanged?.Invoke(currentHour);
                    Debug.Log($"[TimeManager] เวลาปัจจุบัน: {GetTimeString()}");

                    // เช็คว่าถึงเวลาจบเกมหรือยัง
                    if (currentHour >= endHour)
                    {
                        EndGame();
                    }
                }
            }
        }

        private void EndGame()
        {
            gameEnded = true;
            isRunning = false;
            OnGameEnd?.Invoke();
            Debug.Log("[TimeManager] ✓ รอดชีวิต! ถึง 05:00 แล้ว");
        }

        #region Public Methods

        public void StartTime()
        {
            isRunning = true;
            gameEnded = false;
            Debug.Log("[TimeManager] เริ่มนับเวลา");
        }

        public void StopTime()
        {
            isRunning = false;
            Debug.Log("[TimeManager] หยุดนับเวลา");
        }

        public void ResetTime()
        {
            currentHour = startHour;
            currentMinute = 0;
            gameTimer = 0f;
            gameEnded = false;
            Debug.Log("[TimeManager] รีเซ็ตเวลา");
        }

        public string GetTimeString()
        {
            return $"{currentHour:D2}:{currentMinute:D2}";
        }

        /// <summary>
        /// คำนวณโอกาสในการเคลื่อนที่ของ Enemy ตามเวลา
        /// ยิ่งดึกโอกาสยิ่งสูง (01:00 - 05:00)
        /// </summary>
        public float GetEnemyActivityChance()
        {
            if (currentHour == 0)
            {
                if (currentMinute >= 30)
                    return 0.1f;
                return 0f;
            }

            // 01:00 = 20%, 02:00 = 40%, 03:00 = 60%, 04:00 = 80%, 05:00 = 100%
            return Mathf.Clamp01(currentHour * 0.2f);
        }

        /// <summary>
        /// ตั้งค่าอัตราเวลา (1 วิในชีวิตจริง = X วิในเกม)
        /// </summary>
        public void SetTimeScale(float realSecondsPerMinute)
        {
            realSecondsPerGameMinute = Mathf.Max(0.1f, realSecondsPerMinute);
            Debug.Log($"[TimeManager] ตั้งค่า Time Scale: 1 นาทีในเกม = {realSecondsPerGameMinute} วินาที");
        }

        #endregion
    }
}
