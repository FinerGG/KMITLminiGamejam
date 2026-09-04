using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MGJ
{
    /// <summary>
    /// สร้างเอฟเฟกต์กระพริบตา ใช้ตอนเปลี่ยน Environment
    /// ทำให้การเปลี่ยน environment ดูธรรมชาติ
    /// </summary>
    public class BlinkEffect : Singleton<BlinkEffect>
    {
        [Header("UI")]
        [SerializeField] private Canvas blinkCanvas;
        [SerializeField] private Image blinkImage;

        [Header("Blink Settings")]
        [SerializeField] private float blinkDuration = 0.3f;
        [SerializeField] private Color blinkColor = Color.black;

        [Header("Animation Curve")]
        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private void Start()
        {
            // ตั้งค่า canvas ให้ปิดตอนเริ่มต้น
            if (blinkCanvas != null)
            {
                blinkCanvas.gameObject.SetActive(false);
            }

            // ตั้งค่า blinkImage
            if (blinkImage != null)
            {
                blinkImage.color = blinkColor;
            }
        }

        /// <summary>
        /// กระพริบตาแล้วเรียก callback ตอนกลางการกระพริบ
        /// </summary>
        public void Blink(System.Action onMidBlink = null)
        {
            StartCoroutine(BlinkCoroutine(onMidBlink));
        }

        /// <summary>
        /// กระพริบตาพร้อมระยะเวลาที่กำหนด
        /// </summary>
        public void Blink(float duration, System.Action onMidBlink = null)
        {
            StartCoroutine(BlinkCoroutine(onMidBlink, duration));
        }

        private IEnumerator BlinkCoroutine(System.Action onMidBlink, float duration = -1)
        {
            if (duration < 0)
                duration = blinkDuration;

            if (blinkCanvas != null)
                blinkCanvas.gameObject.SetActive(true);

            float halfDuration = duration / 2f;

            // Close eyes (fade to black)
            yield return StartCoroutine(FadeImage(0f, 1f, halfDuration));

            // เรียก callback ตอนปิดตาสนิท
            onMidBlink?.Invoke();

            // รอสักนิด
            yield return new WaitForSeconds(0.1f);

            // Open eyes (fade from black)
            yield return StartCoroutine(FadeImage(1f, 0f, halfDuration));

            if (blinkCanvas != null)
                blinkCanvas.gameObject.SetActive(false);
        }

        private IEnumerator FadeImage(float fromAlpha, float toAlpha, float duration)
        {
            if (blinkImage == null)
                yield break;

            float elapsed = 0f;
            Color color = blinkImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = blinkCurve.Evaluate(t);

                color.a = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
                blinkImage.color = color;

                yield return null;
            }

            // ตั้งค่าสุดท้ายให้แน่ใจ
            color.a = toAlpha;
            blinkImage.color = color;
        }

        /// <summary>
        /// Fade to black แบบช้าๆ
        /// </summary>
        public IEnumerator FadeToBlack(float duration)
        {
            if (blinkCanvas != null)
                blinkCanvas.gameObject.SetActive(true);

            yield return StartCoroutine(FadeImage(0f, 1f, duration));
        }

        /// <summary>
        /// Fade from black แบบช้าๆ
        /// </summary>
        public IEnumerator FadeFromBlack(float duration)
        {
            yield return StartCoroutine(FadeImage(1f, 0f, duration));

            if (blinkCanvas != null)
                blinkCanvas.gameObject.SetActive(false);
        }
    }
}
