using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CCTVDisplay : MonoBehaviour
{
    [Header("UI")]
    public RawImage cameraScreen;
    public TMP_Text cameraNameText;

    public void DisplayCamera(CCTVCameraData cameraData)
    {
        cameraScreen.texture = cameraData.cameraTexture;

        cameraNameText.text = cameraData.cameraName;
    }
}