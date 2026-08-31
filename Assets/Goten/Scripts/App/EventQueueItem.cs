using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MGJ
{
    public class EventQueueItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image selectedIndicator;

        private ThreatEmail email;
        private GmailWindow gmailWindow;

        private void Awake()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnClicked);
            }
        }

        public void Setup(ThreatEmail email, bool isSelected, GmailWindow window)
        {
            this.email = email;
            this.gmailWindow = window;

            if (categoryText != null)
                categoryText.text = "MAIL GATEWAY";

            if (titleText != null)
                titleText.text = email.emailTitle;

            UpdateTime();
            SetSelected(isSelected);
        }

        public void UpdateTime()
        {
            if (timeText != null && email != null)
            {
                int elapsed = Mathf.FloorToInt(Time.time - email.timestamp);
                timeText.text = $"{elapsed}s";
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedIndicator != null)
                selectedIndicator.gameObject.SetActive(isSelected);
        }

        private void OnClicked()
        {
            if (gmailWindow != null && email != null)
            {
                gmailWindow.SelectEmail(email);
            }
        }

        public ThreatEmail Email => email;
    }
}
