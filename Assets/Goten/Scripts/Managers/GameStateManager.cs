using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    /// <summary>
    /// จัดการสถานะเกม
    /// รีเซ็ตเกมโดยไม่โหลด Scene ใหม่
    /// </summary>
    public class GameStateManager : Singleton<GameStateManager>
    {
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Playing;

        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnPlayerDied;
        public UnityEvent OnPlayerWon;
        public UnityEvent OnGameReset;

        public GameState CurrentState => currentState;

        private void Start()
        {
            StartGame();
        }

        #region Game State Control

        public void StartGame()
        {
            currentState = GameState.Playing;

            // เริ่มนับเวลา
            if (TimeManager.Instance != null)
                TimeManager.Instance.StartTime();

            OnGameStart?.Invoke();
            Debug.Log("[GameStateManager] 🎮 เริ่มเกม");
        }

        public void PlayerDied()
        {
            if (currentState == GameState.Dead)
                return;

            currentState = GameState.Dead;

            // หยุดเวลา
            if (TimeManager.Instance != null)
                TimeManager.Instance.StopTime();

            OnPlayerDied?.Invoke();
            Debug.Log("[GameStateManager] ☠ ผู้เล่นตาย");

            // รีเซ็ตหลังจาก 2 วินาที
            Invoke(nameof(ResetGame), 2f);
        }

        public void PlayerWon()
        {
            if (currentState == GameState.Won)
                return;

            currentState = GameState.Won;

            // หยุดเวลา
            if (TimeManager.Instance != null)
                TimeManager.Instance.StopTime();

            OnPlayerWon?.Invoke();
            Debug.Log("[GameStateManager] ✓ ผู้เล่นรอดชีวิต!");
        }

        public void ResetGame()
        {
            currentState = GameState.Playing;

            // รีเซ็ต TimeManager
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ResetTime();
                TimeManager.Instance.StartTime();
            }

            // รีเซ็ต PowerSystem
            if (PowerSystem.Instance != null)
            {
                PowerSystem.Instance.ResetPower();
            }

            // รีเซ็ต Enemy (จะทำในขั้นตอนถัดไป)
            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
            foreach (var enemy in enemies)
            {
                enemy.ResetToStart();
            }

            // รีเซ็ต Doors (จะทำในขั้นตอนถัดไป)
            DoorController[] doors = FindObjectsOfType<DoorController>();
            foreach (var door in doors)
            {
                door.ResetDoor();
            }

            // กล้องกลับไปที่เดิม
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ReCamera();
            }

            OnGameReset?.Invoke();
            Debug.Log("[GameStateManager] 🔄 รีเซ็ตเกม");
        }

        #endregion

        #region Query

        public bool IsPlaying()
        {
            return currentState == GameState.Playing;
        }

        public bool IsDead()
        {
            return currentState == GameState.Dead;
        }

        public bool IsWon()
        {
            return currentState == GameState.Won;
        }

        #endregion
    }

    public enum GameState
    {
        Playing,
        Dead,
        Won
    }
}
