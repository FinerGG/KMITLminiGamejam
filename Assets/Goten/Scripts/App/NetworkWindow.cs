using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace MGJ 
{
    public class NetworkWindow : ResizableWindow
    {
        [SerializeField] private Canvas canvas;

        [Header("Progress")]
        [SerializeField] private Image progressBar;
        [SerializeField] private float progressComplete = 100.0f;

        [Header("DearGoMyLiyKo")]
        [SerializeField] private GameObject DearGoMyLiyKoBox;
        [SerializeField] private List<Sprite> DuckImgs;
        [SerializeField] private GameObject DuckContainer;
        [SerializeField] private GameObject DuckPrefab;
        [SerializeField] private RectTransform DuckSpawnArea; // พื้นที่ spawn ของ duck

        [Header("Events")]
        [SerializeField] private float Cooldown = 1.0f;
        [Range(0.0f, 100.0f)][SerializeField] private float EventPercent = 10.0f;

        private bool isEventActive = false;
        private float progress = 0.0f;
        private float eventTimer = 0.0f;
        private bool finish = false;
        private int duckCounts = 0;
        private List<GameObject> ducks = new List<GameObject>();

        private void Start()
        {
            DearGoMyLiyKoBox.SetActive(false);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameReset.AddListener(RestartProgress);
            }
            else
            {
                Debug.LogWarning("[NetworkWindow] ไม่พบ GameStateManager!");
            }
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isEventActive = !isEventActive;
                    Debug.Log("Event Active: " + isEventActive);
                }

                if (finish)
                {
                    //
                    return;
                }

                if (isEventActive)
                {
                    if (DearGoMyLiyKoBox.activeSelf)
                    {
                        if (duckCounts >= 9)
                            DearGoMyLiyKoEnd();
                        return;
                    }

                    eventTimer += Time.deltaTime;
                    if (eventTimer >= Cooldown)
                    {
                        eventTimer = 0.0f;
                        if (Random.Range(0f, 100f) < EventPercent)
                        {
                            int random = Random.Range(0, 0);
                            switch (random)
                            {
                                case 0: DearGoMyLiyKoStart(); break;
                            }
                        }
                    }
                }

                progress += Time.deltaTime;
                progressBar.fillAmount = (progress / progressComplete);
                if (progress >= progressComplete)
                    finish = true;
            }
        }

        private void DearGoMyLiyKoStart()
        {
            DearGoMyLiyKoBox.SetActive(true);

            // ใช้ DuckSpawnArea แทน canvas ถ้ามีกำหนดไว้
            RectTransform spawnArea = DuckSpawnArea != null ? DuckSpawnArea : canvas.GetComponent<RectTransform>();
            Vector2 spawnSize = spawnArea.sizeDelta;

            // Spawn duck ทั้ง 9 ตัวเลย
            for (int i = 0; i < 9; i++)
            {
                GameObject duck = Instantiate(DuckPrefab, DearGoMyLiyKoBox.transform);
                ducks.Add(duck);

                duck.GetComponent<Image>().sprite = DuckImgs[Random.Range(0, DuckImgs.Count)];

                // สุ่มตำแหน่ง spawn ภายใน spawn area
                RectTransform duckRect = duck.GetComponent<RectTransform>();
                float randomX = Random.Range(-spawnSize.x / 2f + 50f, spawnSize.x / 2f - 50f);
                float randomY = Random.Range(-spawnSize.y / 2f + 50f, spawnSize.y / 2f - 50f);
                duckRect.anchoredPosition = new Vector2(randomX, randomY);

                Duclk duckCom = duck.GetComponent<Duclk>();
                duckCom.SetCanvas(canvas);
                duckCom.OnClick.AddListener(() =>
                {
                    duckCom.RectTransform.SetParent(DuckContainer.transform, false);
                    duckCounts++;
                });
            }
        }

        private void DearGoMyLiyKoEnd()
        {
            // ลบ GameObject ทั้งหมดใน ducks list
            foreach (GameObject duck in ducks)
            {
                if (duck != null)
                {
                    Destroy(duck);
                }
            }

            // Clear list
            ducks.Clear();

            // รีเซ็ต counter
            duckCounts = 0;

            // ปิด DearGoMyLiyKo Box
            DearGoMyLiyKoBox.SetActive(false);
        }

        public void RestartProgress()
        {
            progress = 0.0f;
            finish = false;
            progressBar.fillAmount = 0.0f;
            Debug.Log("Network progress restarted!");
        }

        public void SetCanvas(Canvas canvas)
        {
            this.canvas = canvas;
        }
    }

}