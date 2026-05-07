using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class CombatPromptOpener : MonoBehaviour
{
    public CombatPromptUI promptUI;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        if (promptUI != null)
        {
            // The interactor's root is usually the VR Player
            GameObject player = args.interactorObject.transform.root.gameObject;
            promptUI.ShowUI(player);
        }
    }
}
