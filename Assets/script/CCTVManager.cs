using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    [Header("Camera Data")]
    public CCTVCameraData[] cameras;

    [Header("CCTV Display")]
    public CCTVDisplay display;

    [Header("Current Camera")]
    public int currentCameraIndex = 0;

    private void Start()
    {
        ShowCamera(currentCameraIndex);
    }

    public void NextCamera()
    {
        currentCameraIndex++;

        if (currentCameraIndex >= cameras.Length)
        {
            currentCameraIndex = 0;
        }

        ShowCamera(currentCameraIndex);
    }

    public void PreviousCamera()
    {
        currentCameraIndex--;

        if (currentCameraIndex < 0)
        {
            currentCameraIndex = cameras.Length - 1;
        }

        ShowCamera(currentCameraIndex);
    }

    private void ShowCamera(int index)
    {
        if (cameras.Length == 0)
        {
            return;
        }

        CCTVCameraData camera = cameras[index];

        display.DisplayCamera(camera);
    }
}