using UnityEngine;

namespace MGJ
{
    public class MouseController : Singleton<MouseController>
    {
        [SerializeField] private GameObject reticle;

        private bool isLocked;

        private void Start()
        {
            SetLocked(true);
        }

        private void Update()
        {
            
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;

            Cursor.lockState = locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            Cursor.visible = !locked;

            if (reticle != null)
                reticle.SetActive(locked);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && isLocked)
                SetLocked(true);
        }
    }
}
