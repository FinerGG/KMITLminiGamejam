using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CoolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Settings")]
    [SerializeField] protected Button button;
    [SerializeField] protected float colorFadeSpeed = 6f;
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color hoverColor = Color.red;

    [Header("Scale Settings")]
    [SerializeField] protected float hoverScale = 1.15f; // how big when hovered
    [SerializeField] protected float scaleSpeed = 6f; // smooth speed

    protected TMP_Text buttonText;
    protected Coroutine colorCoroutine;
    protected Coroutine scaleCoroutine;

    private Vector3 originalScale;

    protected virtual void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("SwitchSceneButton: Button component not assigned or found.");
            return;
        }

        buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.color = normalColor;

        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(FadeButtonColor(hoverColor));
        }

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleButton(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(FadeButtonColor(normalColor));
        }

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleButton(originalScale));
    }

    protected virtual IEnumerator FadeButtonColor(Color targetColor)
    {
        if (buttonText == null)
            yield break;

        Color startColor = buttonText.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * colorFadeSpeed;
            buttonText.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        buttonText.color = targetColor;
    }

    protected virtual IEnumerator ScaleButton(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
