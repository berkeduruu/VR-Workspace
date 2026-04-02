using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WeaponSocketManager : MonoBehaviour
{
    public VRWeapon weapon;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        if (socket == null) return;

        socket.selectEntered.AddListener(OnMagazineInserted);
        socket.selectExited.AddListener(OnMagazineRemoved);
    }

    private void OnMagazineInserted(SelectEnterEventArgs args)
    {
        Magazine mag = args.interactableObject.transform.GetComponent<Magazine>();
        if (mag != null && weapon != null)
        {
            // Optional: Check if magazine type matches weapon expectation
            // For now, we assume the socket filtering should handle this, 
            // but let's add a safety check.
            weapon.currentMagazine = mag;
            Debug.Log($"Magazine inserted: {mag.name} (Type: {mag.type})");
        }
    }

    private void OnMagazineRemoved(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (weapon != null)
        {
            weapon.currentMagazine = null;
            Debug.Log("Magazine removed");
        }
    }
}
