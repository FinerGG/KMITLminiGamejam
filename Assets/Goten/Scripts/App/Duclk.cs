using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Duclk : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Button button;
    public UnityEvent OnClick;
    public RectTransform RectTransform => rectTransform;

    [Header("Random Movement")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float changeDirectionInterval = 2f;
    [SerializeField] private float changeDirectionRandomness = 1f;

    private bool isClicked = false;
    private Vector2 currentDirection;
    private float directionTimer;
    private float nextDirectionChangeTime;

    private void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!isClicked)
            {
                isClicked = true;
                OnClick?.Invoke();
            }
        });

        ChangeDirection();
        nextDirectionChangeTime = GetRandomChangeInterval();
    }

    void Update()
    {
        if (isClicked)
            return;

        // สุ่มเปลี่ยนทิศทางตาม interval
        directionTimer += Time.deltaTime;
        if (directionTimer >= nextDirectionChangeTime)
        {
            ChangeDirection();
            directionTimer = 0f;
            nextDirectionChangeTime = GetRandomChangeInterval();
        }

        // เคลื่อนที่
        Vector2 newPosition = rectTransform.anchoredPosition + currentDirection * moveSpeed * Time.deltaTime;

        // จำกัดให้อยู่ใน Canvas
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            float halfWidth = rectTransform.sizeDelta.x / 2f;
            float halfHeight = rectTransform.sizeDelta.y / 2f;

            newPosition.x = Mathf.Clamp(newPosition.x, -canvasSize.x / 2f + halfWidth, canvasSize.x / 2f - halfWidth);
            newPosition.y = Mathf.Clamp(newPosition.y, -canvasSize.y / 2f + halfHeight, canvasSize.y / 2f - halfHeight);

            // ถ้าชนขอบ เปลี่ยนทิศทาง
            if (Mathf.Approximately(newPosition.x, rectTransform.anchoredPosition.x) ||
                Mathf.Approximately(newPosition.y, rectTransform.anchoredPosition.y))
            {
                ChangeDirection();
            }
        }

        rectTransform.anchoredPosition = newPosition;
    }

    private void ChangeDirection()
    {
        // สุ่มทิศทางใหม่ (4 ทิศ: ขึ้น, ลง, ซ้าย, ขวา)
        int randomDir = Random.Range(0, 4);
        switch (randomDir)
        {
            case 0: currentDirection = Vector2.up; break;
            case 1: currentDirection = Vector2.down; break;
            case 2: currentDirection = Vector2.left; break;
            case 3: currentDirection = Vector2.right; break;
        }
    }

    private float GetRandomChangeInterval()
    {
        return changeDirectionInterval + Random.Range(-changeDirectionRandomness, changeDirectionRandomness);
    }

    public void SetCanvas(Canvas canvas)
    {
        this.canvas = canvas;
    }
}
