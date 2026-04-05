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

    // Ghost material logic
    private Material ghostMaterial;
    private Color originalGhostColor;
    private Color extractGhostColor = new Color(1f, 0f, 0f, 0.4f); // Red with transparency

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
        {
            ghostMagazine.SetActive(false);
            Renderer ghostRenderer = ghostMagazine.GetComponent<Renderer>();
            if (ghostRenderer != null && ghostRenderer.material != null)
            {
                ghostMaterial = ghostRenderer.material; // creates an instance
                if (ghostMaterial.HasProperty("_BaseColor"))
                    originalGhostColor = ghostMaterial.GetColor("_BaseColor");
                else if (ghostMaterial.HasProperty("_Color"))
                    originalGhostColor = ghostMaterial.color;
                else
                    originalGhostColor = new Color(0f, 0.5f, 1f, 0.5f); // Default
            }
        }
    }

    void Update()
    {
        if (socket == null || weaponGrab == null) return;

        bool isWeaponHeld = weaponGrab.isSelected;
        bool isMagazineHeldNear = false;
        bool isHandHoveringExtract = false;

        if (!socket.hasSelection)
        {
            // Socket is empty, check if hand with magazine is nearing
            if (socket.hasHover)
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
        }
        else
        {
            // Socket is filled, check if an empty hand (NOT the socket) is approaching it
            IXRSelectInteractable selected = socket.firstInteractableSelected;
            if (selected is XRGrabInteractable grabMag)
            {
                // Check if any interactor hovering the magazine is NOT the socket itself
                foreach (var interactor in grabMag.interactorsHovering)
                {
                    if (interactor is IXRInteractor ixr && (Object)ixr != (Object)socket)
                    {
                        isHandHoveringExtract = true;
                        break;
                    }
                }
            }
        }

        if (ghostMagazine != null)
        {
            bool shouldShowBlue = isWeaponHeld && isMagazineHeldNear && !socket.hasSelection;
            bool shouldShowRed = isHandHoveringExtract;

            if (shouldShowBlue)
            {
                ghostMagazine.SetActive(true);
                SetGhostColor(originalGhostColor);
            }
            else if (shouldShowRed)
            {
                ghostMagazine.SetActive(true);
                SetGhostColor(extractGhostColor);
            }
            else
            {
                ghostMagazine.SetActive(false);
            }
        }
    }

    private void SetGhostColor(Color newColor)
    {
        if (ghostMaterial != null)
        {
            if (ghostMaterial.HasProperty("_BaseColor"))
                ghostMaterial.SetColor("_BaseColor", newColor);
            else if (ghostMaterial.HasProperty("_Color"))
                ghostMaterial.color = newColor;
        }
    }

    private void OnMagazineInserted(SelectEnterEventArgs args)
    {
        Magazine mag = args.interactableObject.transform.GetComponent<Magazine>();
        if (mag != null && weapon != null)
        {
            weapon.currentMagazine = mag;
            if (ghostMagazine != null) ghostMagazine.SetActive(false);
            
            // --- PHYSICS FIX: Ignore collision between Weapon and Magazine to prevent drifting ---
            ToggleCollision(mag, true);
            
            Debug.Log($"Magazine inserted: {mag.name}");
        }
    }

    private void OnMagazineRemoved(SelectExitEventArgs args)
    {
        if (weapon != null)
            weapon.currentMagazine = null;

        Magazine mag = args.interactableObject.transform.GetComponent<Magazine>();
        if (mag != null)
        {
            // Re-enable collision when removed
            ToggleCollision(mag, false);
        }

        if (args.interactableObject is XRGrabInteractable grabInteractable)
        {
            RestoreAttachPoint(grabInteractable);
        }
    }

    private void ToggleCollision(Magazine mag, bool ignore)
    {
        if (weapon == null || mag == null) return;
        
        Collider[] weaponColliders = weapon.GetComponentsInChildren<Collider>();
        Collider[] magColliders = mag.GetComponentsInChildren<Collider>();

        foreach (var wCol in weaponColliders)
        {
            foreach (var mCol in magColliders)
            {
                if (wCol != null && mCol != null)
                    Physics.IgnoreCollision(wCol, mCol, ignore);
            }
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
