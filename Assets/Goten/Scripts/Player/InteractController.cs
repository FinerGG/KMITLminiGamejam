using UnityEngine;

namespace MGJ
{
    public class InteractController : Singleton<InteractController>
    {
        [Header("Raycast")]
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask raycastMask = ~0;

        void Update()
        {
            if (CameraController.Instance.IsCameraTransitioning)
                return;

            Camera cam = CameraController.Instance.Camera;
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(
                    new Vector3(0.5f, 0.5f, 0f)
                );

                if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maxDistance,
                    raycastMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (hit.collider.transform.TryGetComponent<Interactable>(out Interactable inter))
                            inter.Click();
                    }
                }
            }
        }
    }

}