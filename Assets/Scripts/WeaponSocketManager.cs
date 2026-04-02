using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class WeaponSocketManager : MonoBehaviour
{
    public VRWeapon weapon;
    public GameObject ghostMagazine; // Indicator visual
    private XRSocketInteractor socket;
    private XRGrabInteractable weaponGrab;

    // Track active magazines to restore their attach points
    private Dictionary<XRGrabInteractable, Transform> originalAttachPoints = new Dictionary<XRGrabInteractable, Transform>();

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        
        if (weapon != null)
            weaponGrab = weapon.GetComponent<XRGrabInteractable>();

        // --- CORE FIX: Prevent Weapon Grab from stealing the Socket's Trigger Collider ---
        if (weaponGrab != null && (weaponGrab.colliders == null || weaponGrab.colliders.Count == 0))
        {
            Collider[] allChildColliders = weaponGrab.GetComponentsInChildren<Collider>(true);
            foreach (var col in allChildColliders)
            {
                if (col.GetComponent<XRSocketInteractor>() == null && !col.isTrigger)
                {
                    weaponGrab.colliders.Add(col);
                }
            }
        }

        if (socket == null) return;

        // Ensure no magazine is pre-selected on start
        socket.startingSelectedInteractable = null;

        socket.selectEntered.AddListener(OnMagazineInserted);
        socket.selectExited.AddListener(OnMagazineRemoved);
        socket.hoverExited.AddListener(HandleHoverExited);
        
        if (ghostMagazine != null) 
            ghostMagazine.SetActive(false);
    }

    void Update()
    {
        if (socket != null && weaponGrab != null)
        {
            bool isWeaponHeld = weaponGrab.isSelected;
            bool isMagazineHeldNear = false;

            if (socket.hasHover && !socket.hasSelection)
            {
                foreach (var interactable in socket.interactablesHovered)
                {
                    if (interactable is XRGrabInteractable grabInteractable)
                    {
                        // --- DUAL ATTACH LOGIC: Swap to SocketAttach on Hover ---
                        Magazine mag = grabInteractable.GetComponent<Magazine>();
                        if (mag != null && mag.socketAttach != null)
                        {
                            // Store original if not already stored
                            if (!originalAttachPoints.ContainsKey(grabInteractable))
                                originalAttachPoints[grabInteractable] = grabInteractable.attachTransform;

                            // Swap to socket point
                            grabInteractable.attachTransform = mag.socketAttach;
                        }

                        if (grabInteractable.isSelected)
                        {
                            isMagazineHeldNear = true;
                            break;
                        }
                    }
                }
            }

            // RULE 1: The socket only "accepts" the magazine if the weapon itself is being held.
            socket.allowSelect = isWeaponHeld;

            // RULE 2: Show ghost ONLY when weapon is held AND a magazine is being held near the magwell.
            bool shouldGhostBeVisible = isWeaponHeld && isMagazineHeldNear;

            if (ghostMagazine != null)
            {
                if (ghostMagazine.activeSelf != shouldGhostBeVisible)
                {
                    ghostMagazine.SetActive(shouldGhostBeVisible);
                }
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

        if (args.interactableObject is XRGrabInteractable grabInteractable)
        {
            RestoreAttachPoint(grabInteractable);
        }
    }

    private void RestoreAttachPoint(XRGrabInteractable grabInteractable)
    {
        // Restore to original hand attach point
        if (originalAttachPoints.TryGetValue(grabInteractable, out Transform original))
        {
            grabInteractable.attachTransform = original;
            originalAttachPoints.Remove(grabInteractable);
        }
        else
        {
            // Fallback: If not in dictionary, try to find the HandAttach on the Magazine itself
            Magazine mag = grabInteractable.GetComponent<Magazine>();
            if (mag != null && mag.handAttach != null)
            {
                grabInteractable.attachTransform = mag.handAttach;
            }
        }
    }

    // Cleanup on Hover Exit to restore hand-durus if user pulls away without snapping
    public void HandleHoverExited(HoverExitEventArgs args)
    {
        if (args.interactableObject is XRGrabInteractable grabInteractable)
        {
            RestoreAttachPoint(grabInteractable);
        }
    }
}
