using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    /// <summary>
    /// จัดการพฤติกรรมของ Enemy เมื่ออยู่ที่ประตู
    /// - ได้ยินเสียงเท้า
    /// - ถ้าไม่ล็อค → Enemy เข้ามา → ตาย
    /// - ถ้าล็อค → เสียงเบาลง → Enemy กลับไป
    /// </summary>
    public class EnemyAtDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyAI enemyAI;
        [SerializeField] private DoorController targetDoor; // ประตูที่ Enemy จะโจมตี
        [SerializeField] private AudioSource footstepAudioSource;

        [Header("Audio")]
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private float maxFootstepVolume = 1f;
        [SerializeField] private float fadeSpeed = 1f; // ความเร็วในการ fade in/out

        [Header("Attack Settings")]
        [SerializeField] private float attackDelay = 5f; // เวลาที่ Enemy จะเข้ามา (วินาที)

        [Header("Events")]
        public UnityEvent OnEnemyArrive; // เรียกเมื่อ Enemy มาถึงประตู
        public UnityEvent OnPlayerAttacked; // เรียกเมื่อโจมตีผู้เล่น
        public UnityEvent OnEnemyLeave; // เรียกเมื่อ Enemy กลับไป

        private bool isWaiting = false; // กำลังรออยู่ที่ประตูหรือไม่
        private float attackTimer = 0f;
        private Coroutine fadeCoroutine;
        private Coroutine attackCoroutine;

        private void Awake()
        {
            if (enemyAI == null)
                enemyAI = GetComponent<EnemyAI>();

            if (footstepAudioSource != null)
            {
                footstepAudioSource.clip = footstepSound;
                footstepAudioSource.loop = true;
                footstepAudioSource.volume = 0f;
            }
        }

        private void Start()
        {
            // Subscribe to Enemy events
            if (enemyAI != null)
            {
                enemyAI.OnReachDoor.AddListener(OnEnemyReachDoor);
                enemyAI.OnReset.AddListener(OnEnemyReset);
            }
        }

        private void Update()
        {
            // ถ้า Enemy อยู่ที่ประตู และผู้เล่นอยู่ที่ประตูด้วย
            if (isWaiting && targetDoor != null && targetDoor.IsPlayerAtDoor)
            {
                // ถ้าประตูถูกล็อค → เสียงเบาลง → Enemy กลับไป
                if (targetDoor.IsLocked)
                {
                    EnemyLeave();
                }
            }
        }

        #region Enemy Behavior

        /// <summary>
        /// เรียกเมื่อ Enemy มาถึงประตู
        /// </summary>
        private void OnEnemyReachDoor()
        {
            if (isWaiting)
                return;

            isWaiting = true;
            attackTimer = 0f;

            OnEnemyArrive?.Invoke();
            Debug.Log($"[EnemyAtDoor] Enemy มาถึงประตู: {targetDoor?.gameObject.name}");

            // เริ่มนับถอยหลังโจมตี
            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);

            attackCoroutine = StartCoroutine(AttackCountdown());
        }

        /// <summary>
        /// นับถอยหลังโจมตีผู้เล่น
        /// </summary>
        private IEnumerator AttackCountdown()
        {
            // รอจนกว่าผู้เล่นจะเข้ามาที่ประตู
            while (targetDoor != null && !targetDoor.IsPlayerAtDoor)
            {
                yield return null;
            }

            // ผู้เล่นเข้ามาที่ประตูแล้ว → เริ่มเล่นเสียงเท้า
            PlayFootsteps(true);

            // นับถอยหลัง
            float elapsed = 0f;
            while (elapsed < attackDelay)
            {
                // ถ้าประตูถูกล็อค → ยกเลิก
                if (targetDoor != null && targetDoor.IsLocked)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // ครบเวลาแล้ว → โจมตีผู้เล่น
            AttackPlayer();
        }

        /// <summary>
        /// โจมตีผู้เล่น
        /// </summary>
        private void AttackPlayer()
        {
            isWaiting = false;

            // หยุดเสียงเท้า
            PlayFootsteps(false);

            OnPlayerAttacked?.Invoke();

            // แจ้ง GameStateManager ว่าผู้เล่นตาย
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PlayerDied();
            }

            Debug.Log($"[EnemyAtDoor] ☠ Enemy โจมตีผู้เล่น!");
        }

        /// <summary>
        /// Enemy กลับไปจุดเริ่มต้น (เมื่อประตูถูกล็อค)
        /// </summary>
        private void EnemyLeave()
        {
            isWaiting = false;

            // หยุดการโจมตี
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            // Fade out เสียงเท้า
            PlayFootsteps(false);

            // Enemy กลับไปจุดเริ่มต้น
            if (enemyAI != null)
            {
                enemyAI.ResetToStart();
            }

            OnEnemyLeave?.Invoke();
            Debug.Log($"[EnemyAtDoor] Enemy กลับไปจุดเริ่มต้น");
        }

        /// <summary>
        /// เรียกเมื่อ Enemy รีเซ็ต
        /// </summary>
        private void OnEnemyReset()
        {
            isWaiting = false;
            attackTimer = 0f;

            // หยุดเสียงทั้งหมด
            PlayFootsteps(false);

            // หยุด Coroutine
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }

        #endregion

        #region Audio Control

        /// <summary>
        /// เล่นเสียงเท้า (fade in/out)
        /// </summary>
        private void PlayFootsteps(bool play)
        {
            if (footstepAudioSource == null)
                return;

            // หยุด Coroutine เดิม
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (play)
            {
                // Fade in
                if (!footstepAudioSource.isPlaying)
                    footstepAudioSource.Play();

                fadeCoroutine = StartCoroutine(FadeAudio(maxFootstepVolume));
            }
            else
            {
                // Fade out
                fadeCoroutine = StartCoroutine(FadeAudio(0f));
            }
        }

        /// <summary>
        /// Fade เสียง
        /// </summary>
        private IEnumerator FadeAudio(float targetVolume)
        {
            while (Mathf.Abs(footstepAudioSource.volume - targetVolume) > 0.01f)
            {
                footstepAudioSource.volume = Mathf.MoveTowards(
                    footstepAudioSource.volume,
                    targetVolume,
                    fadeSpeed * Time.deltaTime
                );
                yield return null;
            }

            footstepAudioSource.volume = targetVolume;

            // ถ้า fade out เสร็จแล้ว → หยุดเล่น
            if (targetVolume == 0f && footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }

            fadeCoroutine = null;
        }

        #endregion
    }
}
