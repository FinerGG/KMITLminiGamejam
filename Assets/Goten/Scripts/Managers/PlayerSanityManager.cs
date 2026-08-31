using UnityEngine;
using UnityEngine.Events;

namespace MGJ
{
    public class PlayerSanityManager : Singleton<PlayerSanityManager>
    {
        [Header("Sanity Settings")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity = 100f;
        [SerializeField] private float minSanity = 0f;

        [Header("Events")]
        public UnityEvent<float> onSanityChanged; // Passes current sanity percentage (0-1)
        public UnityEvent onSanityDepleted;

        private void Start()
        {
            currentSanity = maxSanity;
            NotifySanityChanged();
        }

        public void DecreaseSanity(float amount)
        {
            currentSanity = Mathf.Max(minSanity, currentSanity - amount);
            NotifySanityChanged();

            if (currentSanity <= minSanity)
            {
                onSanityDepleted?.Invoke();
                Debug.Log("Player sanity depleted!");
            }

            Debug.Log($"Sanity decreased by {amount}. Current: {currentSanity}/{maxSanity}");
        }

        public void IncreaseSanity(float amount)
        {
            currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
            NotifySanityChanged();

            Debug.Log($"Sanity increased by {amount}. Current: {currentSanity}/{maxSanity}");
        }

        public void RestoreSanity()
        {
            currentSanity = maxSanity;
            NotifySanityChanged();
            Debug.Log("Sanity fully restored!");
        }

        private void NotifySanityChanged()
        {
            float percentage = currentSanity / maxSanity;
            onSanityChanged?.Invoke(percentage);
        }

        // Properties
        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public float SanityPercentage => currentSanity / maxSanity;
        public bool IsDepleted => currentSanity <= minSanity;
    }
}
