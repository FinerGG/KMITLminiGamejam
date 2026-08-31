using System.Collections;
using UnityEngine;

public class ExitUIButton : CoolButton
{
    [SerializeField] private GameObject UI;

    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (UI != null)
        {
            UI.SetActive(false);
            buttonText.color = normalColor;
        }
        else
        {
#if UNITY_EDITOR
            // If running inside Unity editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as built game
        Application.Quit();
#endif
        }
    }
}