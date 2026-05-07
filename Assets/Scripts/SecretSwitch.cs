using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(BoxCollider))]
public class SecretSwitch : MonoBehaviour
{
    public SecretDoor targetDoor;

    void Awake()
    {
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (targetDoor != null)
        {
            targetDoor.ToggleDoor();
        }
    }
}
