using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WeaponSocketManager : MonoBehaviour
{
    public VRWeapon weapon;
    public GameObject ghostMagazine; // Indicator visual
    private XRSocketInteractor socket;
    private XRGrabInteractable weaponGrab;

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
                        if (grabInteractable.isSelected)
                        {
                            isMagazineHeldNear = true;
                            break;
                        }
                    }
                }
            }

            // RULE 1: The socket only "accepts" the magazine if the weapon itself is being held.
            // This allows snapping on release because the weapon is still held when the magazine is let go.
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
    }
}
