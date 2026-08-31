using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MGJ 
{
    public class ComputerController : Interactable
    {
        [Header("Components")]
        [SerializeField] private Camera _computerCamera;
        [SerializeField] private GameObject _appsFrame;

        private bool _connected = false;
        private void Start()
        {
            foreach (Button Bu in _appsFrame.GetComponentsInChildren<Button>())
            {
                if (Bu.TryGetComponent<App>(out App app))
                {
                    Bu.onClick.RemoveAllListeners();
                    Bu.onClick.AddListener(() => {
                        app.Open();
                    });
                }
            }
        }

        void Update()
        {
            if (!active)
            {
                if (_connected)
                {
                    _connected = false;
                    CameraController.Instance.ReCamera();
                    MouseController.Instance.SetLocked(true);
                }
                return;
            }

            if (_connected)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    _connected = false;
                    CameraController.Instance.ReCamera();
                    MouseController.Instance.SetLocked(true);
                    return;
                }

                // Manual click detection for UI with small scale
                if (Input.GetMouseButtonDown(0) && _computerCamera != null)
                {
                    Ray ray = _computerCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100f, 1 << 7)) // Layer 7
                    {
                        Button button = hit.collider.GetComponent<Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                        }
                    }
                }
            }

            if (interact)
            {
                interact = false;
                if (_computerCamera == null)
                {
                    Debug.LogWarning("ComputerController computer camera is null.");
                    return;
                }
                CameraController.Instance.SetCameraTurnEnable(false, new Vector3(0,0,0));
                CameraController.Instance.SetCamera(_computerCamera);
                MouseController.Instance.SetLocked(false);
                _connected = true;
            }
        }
    }

}