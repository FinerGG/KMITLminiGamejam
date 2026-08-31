using UnityEngine.UI;
using UnityEngine;

namespace MGJ 
{
    public class NetworkWindow : ResizableWindow
    {
        [Header("Progress")]
        [SerializeField] private Image progressBar;
        [SerializeField] private float progressComplete = 100.0f;

        [Header("CheckRobot")]
        [SerializeField] private GameObject RobotBox;
        [SerializeField] private Button CheckRobotButton;
        [SerializeField] private float Cooldown = 1.0f;
        [Range(0.0f, 100.0f)][SerializeField] private float RobotPercent = 10.0f;

        private float progress = 0.0f;
        private float robotTimer = 0.0f;
        private bool finish = false;

        private void Start()
        {
            RobotBox.SetActive(false);
            CheckRobotButton.onClick.RemoveAllListeners();
            CheckRobotButton.onClick.AddListener(() =>
            {
                RobotBox.SetActive(false);
            });
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                robotTimer += Time.deltaTime;
                if (robotTimer >= Cooldown)
                {
                    robotTimer = 0.0f;
                    if (Random.Range(0f, 100f) < RobotPercent)
                    {
                        RobotBox.SetActive(true);
                    }
                }

                if (RobotBox.activeSelf)
                    return;

                if (finish)
                {
                    //
                    return;
                }

                progress += Time.deltaTime;
                progressBar.fillAmount = (progress / progressComplete);
                if (progress >= progressComplete)
                    finish = true;
            }
        }

        public void RestartProgress()
        {
            progress = 0.0f;
            finish = false;
            progressBar.fillAmount = 0.0f;
            Debug.Log("Network progress restarted!");
        }
    }

}