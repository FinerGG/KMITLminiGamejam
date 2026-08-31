using Unity.VisualScripting;
using UnityEngine;

public class Flashlight : Interactable
{
    [SerializeField] private KeyCode actionKey;
    private GameObject _flashlight;

    protected override void Update()
    {
        base.Update();
        if (canAction)
        {
            if (Input.GetKeyDown(actionKey))
            {
                _flashlight.SetActive(!_flashlight.activeSelf);
            }
        }
    }

    public override void Use()
    {
        base.Use();
        lastInteract.transform.AddComponent<Flashlight>();

        Flashlight FL = lastInteract.transform.GetComponent<Flashlight>();
        FL.setup(this);
        FL.setFlashlight(lastInteract.Flashlight);

        Destroy(UI);
        Destroy(gameObject);
    }

    public void setup(Flashlight flashlight)
    {
        actionKey = flashlight.actionKey;
    }

    public void setFlashlight(GameObject flashlight)
    {
        canAction = true;
        _flashlight = flashlight;
    }
}
