using Unity.VisualScripting;
using UnityEngine;

public class OpenUIButton : CoolButton
{
    [SerializeField] private GameObject UI;

    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(OpenUI);
    }

    private void OpenUI()
    {
        UI.SetActive(true);
    }
}