using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float Distance = 2f;
    [SerializeField] protected GameObject UI;
    [SerializeField] private LayerMask playerMask;

    private Transform camTrans;
    protected bool canAction = false;
    protected Interact lastInteract;

    private void Start()
    {
        if (UI != null)
        {
            UI.transform.parent = UIManager.Instance.transform;
            camTrans = Camera.main.transform;
        }
    }

    protected virtual void Update()
    {
        if (!canAction)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Distance, playerMask);
            if (colliders.Length > 0)
            {
                GameObject hit = colliders[0].gameObject;
                if (hit.TryGetComponent<Interact>(out Interact interact) && lastInteract == null)
                {
                    lastInteract = interact;
                    if (interact.nearstInteractable == null)
                        interact.setNearst(this);
                }
            }
            else if (lastInteract != null)
            {
                ClearUI();
                lastInteract.setNearst(null);
                lastInteract = null;
            }
        }
    }

    private void LateUpdate()
    {
        if (canAction) return;
        if (!UI.activeSelf) return;

        UI.transform.LookAt(
            UI.transform.position + camTrans.forward,
            camTrans.up
        );
    }

    private void OnDrawGizmos()
    {
        if (canAction) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Distance);
    }

    public virtual void Use()
    {

    }

    public void DisplayUI(Transform transform)
    {
        UI.SetActive(true); 
    }

    private void ClearUI()
    {
        UI.SetActive(false);
    }
}
