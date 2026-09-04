using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    public class EnemyAI : Singleton<EnemyAI>
    {
        [Header("Waypoints")]
        [SerializeField] private Transform startWaypoint; // จุดเริ่มต้น (แยกจาก waypoints list)
        [SerializeField] private List<Transform> waypoints = new List<Transform>(); // Pattern: 1 → 2 → 3 → Door (ไม่รวม Start)
        [SerializeField] private int currentWaypointIndex = 0;

        [Header("Movement Settings")]
        [SerializeField] private float moveInterval = 5f; // ระยะเวลาระหว่างการพยายามเคลื่อนที่ (วินาทีจริง)
        [SerializeField] private float moveIntervalRandomness = 2f; // ความสุ่มของ interval
        [SerializeField] private float movementChanceMultiplier = 1f; // คูณกับ TimeManager.GetEnemyActivityChance()

        [Header("State")]
        [SerializeField] private bool isBeingWatched = false; // ถูกมองผ่านกล้องหรือไม่
        [SerializeField] private bool isAtDoor = false; // อยู่ที่ประตูหรือยัง
        [SerializeField] private bool isActive = false; // เริ่มเคลื่อนที่แล้วหรือยัง

        [Header("Events")]
        public UnityEvent OnMove; // เรียกเมื่อเคลื่อนที่
        public UnityEvent OnReachDoor; // เรียกเมื่อถึงประตู
        public UnityEvent OnReset; // เรียกเมื่อรีเซ็ต

        private float moveTimer = 0f;
        private float nextMoveTime = 0f;

        public bool IsAtDoor => isAtDoor;
        public bool IsBeingWatched => isBeingWatched;
        public int CurrentWaypointIndex => currentWaypointIndex;
        public Transform CurrentWaypoint => currentWaypointIndex < waypoints.Count ? waypoints[currentWaypointIndex] : startWaypoint;

        private void Start()
        {
            ResetToStart();
            nextMoveTime = GetRandomMoveInterval();
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying())
                return;

            if (!isActive || isAtDoor)
                return;

            moveTimer += Time.deltaTime;

            if (moveTimer >= nextMoveTime)
            {
                moveTimer = 0f;
                nextMoveTime = GetRandomMoveInterval();
                TryMove();
            }
        }

        #region Movement Logic

        /// <summary>
        /// พยายามเคลื่อนที่ (สุ่มตาม TimeManager)
        /// </summary>
        private void TryMove()
        {
            // ถ้าถูกมอง ไม่สามารถเคลื่อนที่ได้
            if (isBeingWatched)
            {
                Debug.Log($"[EnemyAI] {gameObject.name} ถูกมอง! ไม่สามารถเคลื่อนที่ได้");
                return;
            }

            // สุ่มว่าจะเคลื่อนที่หรือไม่ (ตาม TimeManager * Multiplier)
            float activityChance = TimeManager.Instance.GetEnemyActivityChance() * movementChanceMultiplier;
            float roll = Random.value;

            if (roll <= activityChance)
            {
                WarpToNextWaypoint();
            }
            else
            {
                Debug.Log($"[EnemyAI] {gameObject.name} ไม่เคลื่อนที่ (roll: {roll:F2} > chance: {activityChance:F2})");
            }
        }

        /// <summary>
        /// วาปไปจุดถัดไป
        /// </summary>
        private void WarpToNextWaypoint()
        {
            // เช็คว่าถึงจุดสุดท้ายหรือยัง
            if (currentWaypointIndex >= waypoints.Count - 1)
            {
                // ถึงประตูแล้ว
                isAtDoor = true;
                OnReachDoor?.Invoke();
                Debug.Log($"[EnemyAI] {gameObject.name} ถึงประตูแล้ว!");
                return;
            }

            // วาปไปจุดถัดไป
            currentWaypointIndex++;
            transform.position = waypoints[currentWaypointIndex].position;
            transform.rotation = waypoints[currentWaypointIndex].rotation;

            OnMove?.Invoke();
            Debug.Log($"[EnemyAI] {gameObject.name} วาปไป Waypoint {currentWaypointIndex}");
        }

        /// <summary>
        /// กลับไปจุดเริ่มต้น
        /// </summary>
        public void ResetToStart()
        {
            currentWaypointIndex = 0;
            isAtDoor = false;
            isBeingWatched = false;
            moveTimer = 0f;

            if (startWaypoint != null)
            {
                transform.position = startWaypoint.position;
                transform.rotation = startWaypoint.rotation;
            }

            OnReset?.Invoke();
            Debug.Log($"[EnemyAI] {gameObject.name} กลับไปจุดเริ่มต้น");
        }

        #endregion

        #region Camera Detection

        /// <summary>
        /// เซ็ตว่าถูกมองผ่านกล้องหรือไม่
        /// </summary>
        public void SetBeingWatched(bool watched)
        {
            isBeingWatched = watched;

            if (watched)
            {
                Debug.Log($"[EnemyAI] {gameObject.name} กำลังถูกมอง!");
            }
        }

        #endregion

        #region Helper Methods

        private float GetRandomMoveInterval()
        {
            return moveInterval + Random.Range(-moveIntervalRandomness, moveIntervalRandomness);
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0)
                return;

            // วาดเส้นทาง
            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            // วาด Waypoint ปัจจุบัน
            if (Application.isPlaying && CurrentWaypoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(CurrentWaypoint.position, 0.5f);
            }
        }

        #endregion
    }
}
