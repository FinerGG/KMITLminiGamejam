using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private KeyCode key;
    [SerializeField] private GameObject flashlight;

    private Interactable _nearstInteractable;
    public Interactable nearstInteractable => _nearstInteractable;
    public GameObject Flashlight => flashlight;


    private void Update()
    {
        if (_nearstInteractable != null)
        {
            _nearstInteractable.DisplayUI(transform);
            if (Input.GetKeyDown(key))
            {
                _nearstInteractable.Use();
            }
        }
    }

    public void setNearst(Interactable interactable)
    {
        _nearstInteractable = interactable;
    }
}
