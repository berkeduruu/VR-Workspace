using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WeaponSocketManager : MonoBehaviour
{
    public VRWeapon weapon;
    public GameObject ghostMagazine; // Indicator visual
    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        
        // --- CORE FIX: Prevent Weapon Grab from stealing the Socket's Trigger Collider ---
        XRGrabInteractable parentGrab = GetComponentInParent<XRGrabInteractable>();
        if (parentGrab != null && parentGrab.colliders.Count == 0)
        {
            Collider[] allChildColliders = parentGrab.GetComponentsInChildren<Collider>(true);
            foreach (var col in allChildColliders)
            {
                // Add all colliders to the weapon's grab EXCEPT this socket's collider
                if (col.GetComponent<XRSocketInteractor>() == null)
                {
                    parentGrab.colliders.Add(col);
                }
            }
        }

        if (socket == null) return;

        socket.selectEntered.AddListener(OnMagazineInserted);
        socket.selectExited.AddListener(OnMagazineRemoved);
        
        if (ghostMagazine != null) 
            ghostMagazine.SetActive(false);
    }

    void Update()
    {
        if (socket != null && ghostMagazine != null)
        {
            // Only show the ghost when a magazine is hovering AND it is currently being held.
            bool shouldGhostBeVisible = false;

            if (socket.hasHover && !socket.hasSelection)
            {
                foreach (var interactable in socket.interactablesHovered)
                {
                    if (interactable is XRGrabInteractable grabInteractable)
                    {
                        // Check if the magazine is actively held by a hand
                        if (grabInteractable.isSelected)
                        {
                            shouldGhostBeVisible = true;
                            break; // Show ghost!
                        }
                    }
                }
            }

            // Toggle visibility to match the held state
            if (ghostMagazine.activeSelf != shouldGhostBeVisible)
            {
                ghostMagazine.SetActive(shouldGhostBeVisible);
            }
        }
    }

    private void OnMagazineInserted(SelectEnterEventArgs args)
    {
        Magazine mag = args.interactableObject.transform.GetComponent<Magazine>();
        if (mag != null && weapon != null)
        {
            weapon.currentMagazine = mag;
            if (ghostMagazine != null) ghostMagazine.SetActive(false);
            Debug.Log($"Magazine inserted: {mag.name}");
        }
    }

    private void OnMagazineRemoved(SelectExitEventArgs args)
    {
        if (weapon != null)
            weapon.currentMagazine = null;
    }
}
