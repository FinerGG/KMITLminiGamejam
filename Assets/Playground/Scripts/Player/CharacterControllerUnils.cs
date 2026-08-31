using UnityEngine;

public class CharacterControllerUnils : MonoBehaviour
{
    public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layerMask = default)
    {
        Vector3 normal = Vector3.up;
        Vector3 center = characterController.transform.position + characterController.center;
        float dist = characterController.height / 2f + characterController.stepOffset + 0.01f;

        RaycastHit hit;
        if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, dist, layerMask))
        {
            normal = hit.normal;
        }

        return normal;
    }
}
