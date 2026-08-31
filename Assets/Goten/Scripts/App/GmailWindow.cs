using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MGJ
{
    public class GmailWindow : ResizableWindow
    {
        [Header("Gmail UI References")]
        [SerializeField] private TextMeshProUGUI caseNumberText;
        [SerializeField] private TextMeshProUGUI emailTitleText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI sourceValueText;
        [SerializeField] private TextMeshProUGUI capturedDataValueText;
        [SerializeField] private Transform signalsContainer;
        [SerializeField] private GameObject signalRowPrefab;

        [Header("Action Buttons")]
        [SerializeField] private Button quarantineButton;
        [SerializeField] private Button allowButton;

        [Header("Event Queue")]
        [SerializeField] private Transform eventQueueContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private TextMeshProUGUI activeCountText;

        [Header("Status Bar")]
        [SerializeField] private TextMeshProUGUI eventsResolvedText;

        private List<ThreatEmail> emailQueue = new List<ThreatEmail>();
        private List<EventQueueItem> queueItemsList = new List<EventQueueItem>();
        private ThreatEmail currentEmail;
        private float emailStartTime;
        private int resolvedCount = 0;

        // Reference to game manager for callbacks
        private System.Action<bool> onQuarantine; // true = correct decision
        private System.Action<bool> onAllow; // true = correct decision

        protected override void Awake()
        {
            base.Awake();

            if (quarantineButton != null)
            {
                quarantineButton.onClick.AddListener(OnQuarantineClicked);
            }

            if (allowButton != null)
            {
                allowButton.onClick.AddListener(OnAllowClicked);
            }
        }

        private void Update()
        {
            if (currentEmail != null && !currentEmail.isResolved)
            {
                UpdateTimer();
            }
        }

        public void Initialize(System.Action<bool> onQuarantineCallback, System.Action<bool> onAllowCallback)
        {
            onQuarantine = onQuarantineCallback;
            onAllow = onAllowCallback;
        }

        public void AddEmail(ThreatEmail email)
        {
            emailQueue.Add(email);
            UpdateEventQueue();

            if (currentEmail == null)
            {
                DisplayEmail(email);
            }
        }

        public void SelectEmail(ThreatEmail email)
        {
            if (email == null || email.isResolved) return;

            DisplayEmail(email);
            UpdateEventQueue();
        }

        private void DisplayEmail(ThreatEmail email)
        {
            currentEmail = email;
            emailStartTime = Time.time;

            // Update UI
            if (caseNumberText != null)
                caseNumberText.text = $"MAIL GATEWAY / CASE {email.caseNumber}";

            if (emailTitleText != null)
                emailTitleText.text = email.emailTitle;

            if (sourceValueText != null)
                sourceValueText.text = email.source;

            if (capturedDataValueText != null)
                capturedDataValueText.text = email.capturedData;

            // Display signals
            DisplaySignals(email.observedSignals);

            // Update active count
            UpdateActiveCount();
        }

        private void DisplaySignals(List<string> signals)
        {
            // Clear existing signals
            foreach (Transform child in signalsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create signal rows
            for (int i = 0; i < signals.Count; i++)
            {
                GameObject row = Instantiate(signalRowPrefab, signalsContainer);
                SignalRow signalRow = row.GetComponent<SignalRow>();
                if (signalRow != null)
                {
                    signalRow.Setup(i + 1, signals[i]);
                }
            }
        }

        private void UpdateTimer()
        {
            if (timerText != null)
            {
                int elapsed = Mathf.FloorToInt(Time.time - emailStartTime);
                timerText.text = $"{elapsed} SEC";
            }
        }

        private void UpdateEventQueue()
        {
            // Clear and rebuild queue UI
            foreach (Transform child in eventQueueContainer)
            {
                Destroy(child.gameObject);
            }

            queueItemsList.Clear();

            foreach (var email in emailQueue)
            {
                if (!email.isResolved)
                {
                    GameObject item = Instantiate(eventItemPrefab, eventQueueContainer);
                    EventQueueItem queueItem = item.GetComponent<EventQueueItem>();
                    if (queueItem != null)
                    {
                        queueItem.Setup(email, email == currentEmail, this);
                        queueItemsList.Add(queueItem);
                    }
                }
            }
        }

        private void UpdateActiveCount()
        {
            int activeCount = 0;
            foreach (var email in emailQueue)
            {
                if (!email.isResolved) activeCount++;
            }

            if (activeCountText != null)
            {
                activeCountText.text = $"{activeCount} ACTIVE";
            }
        }

        private void OnQuarantineClicked()
        {
            if (currentEmail == null) return;

            bool isCorrect = currentEmail.isVirus; // Correct if it's actually a virus
            currentEmail.isResolved = true;
            resolvedCount++;

            // Callback
            onQuarantine?.Invoke(isCorrect);

            UpdateResolvedCount();
            NextEmail();
        }

        private void OnAllowClicked()
        {
            if (currentEmail == null) return;

            bool isCorrect = !currentEmail.isVirus; // Correct if it's safe
            currentEmail.isResolved = true;
            resolvedCount++;

            // Callback
            onAllow?.Invoke(isCorrect);

            UpdateResolvedCount();
            NextEmail();
        }

        private void NextEmail()
        {
            // Find next unresolved email
            ThreatEmail nextEmail = null;
            foreach (var email in emailQueue)
            {
                if (!email.isResolved)
                {
                    nextEmail = email;
                    break;
                }
            }

            if (nextEmail != null)
            {
                DisplayEmail(nextEmail);
            }
            else
            {
                currentEmail = null;
                ClearDisplay();
            }

            UpdateEventQueue();
        }

        private void ClearDisplay()
        {
            if (emailTitleText != null)
                emailTitleText.text = "No active threats";
        }

        private void UpdateResolvedCount()
        {
            if (eventsResolvedText != null)
            {
                eventsResolvedText.text = $"{resolvedCount} EVENTS RESOLVED";
            }
        }
    }
}
