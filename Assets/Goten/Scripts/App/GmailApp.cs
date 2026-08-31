using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    public class GmailApp : App
    {
        [Header("Gmail Settings")]
        [SerializeField] private EmailGeneratorConfig emailConfig;
        [SerializeField] private float emailSpawnInterval = 30f; // Spawn new email every 30 seconds
        [SerializeField] private float virusProbability = 0.5f; // 50% chance of virus

        [Header("References")]
        [SerializeField] private App networkApp; // Reference to Network App

        private EmailGenerator emailGenerator;
        private float nextSpawnTime;
        private GmailWindow gmailWindow;

        private void Start()
        {
            if (emailConfig != null)
            {
                emailGenerator = new EmailGenerator(emailConfig);
            }

            if (gmailWindow != null)
            {
                Open();
                Close();
            }
        }

        private void Update()
        {
            if (isOpen && Time.time >= nextSpawnTime)
            {
                SpawnRandomEmail();
                nextSpawnTime = Time.time + emailSpawnInterval;
            }
        }

        protected override void OnWindowOpened(Window window)
        {
            gmailWindow = window as GmailWindow;
            if (gmailWindow != null)
            {
                gmailWindow.Initialize(OnQuarantineDecision, OnAllowDecision);

                // Spawn first email immediately
                SpawnRandomEmail();
                nextSpawnTime = Time.time + emailSpawnInterval;
            }
        }

        private void SpawnRandomEmail()
        {
            if (emailGenerator == null || gmailWindow == null) return;

            // Random chance of virus based on probability
            bool isVirus = Random.value < virusProbability;
            ThreatEmail email = emailGenerator.GenerateEmail(isVirus);

            gmailWindow.AddEmail(email);
        }

        private void OnQuarantineDecision(bool isCorrect)
        {
            if (isCorrect)
            {
                // Correct - virus quarantined
                Debug.Log("✓ Correct! Virus quarantined.");
            }
            else
            {
                // Wrong - safe email quarantined
                Debug.Log("✗ Wrong! Safe email quarantined. Sanity decreased.");
                PlayerSanityManager.Instance?.DecreaseSanity(10f);
            }
        }

        private void OnAllowDecision(bool isCorrect)
        {
            if (isCorrect)
            {
                // Correct - safe email allowed
                Debug.Log("✓ Correct! Safe email allowed.");
            }
            else
            {
                // Wrong - virus allowed
                Debug.Log("✗ Wrong! Virus allowed. Network progress restarted.");
                if (networkApp != null && networkApp.CurrentWindow != null)
                {
                    NetworkWindow networkWindow = networkApp.CurrentWindow as NetworkWindow;
                    if (networkWindow != null)
                    {
                        networkWindow.RestartProgress();
                    }
                }
            }
        }
    }
}
