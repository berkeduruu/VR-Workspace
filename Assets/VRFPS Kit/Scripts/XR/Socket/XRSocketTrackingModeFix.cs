using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Sockets override movement mode to instantaneous but doesnt reset until item is completely dropped. This is a fix for that
[RequireComponent(typeof(XRBaseInteractor))] [DisallowMultipleComponent]
public class XRSocketTrackingModeFix : MonoBehaviour
{
    private XRBaseInteractable.MovementType _previousMovementType;
    
    void Awake()
    {
        XRBaseInteractor interactor = GetComponent<XRBaseInteractor>();
        interactor.selectEntered.AddListener((args) => 
        {
            if(args.interactableObject is not XRGrabInteractable grabbable) return;
            _previousMovementType = grabbable.movementType;
        });
        interactor.selectExited.AddListener((args) =>
        {
            if(args.interactableObject is not XRGrabInteractable grabbable) return;
            grabbable.movementType = _previousMovementType;
            
            //Reset kinematic state in case it was changed by socket movementType (usually to kinematic which makes item kinematic)
            grabbable.GetComponent<Rigidbody>().isKinematic = false;
            //Also fix the internal wasKinematic value through reflection
            SetGrabbableInternalValue_WasKinematic(grabbable, false);
        });

    }
    
    private void SetGrabbableInternalValue_WasKinematic(XRGrabInteractable grabbable, bool wasKinematic)
    {
        FieldInfo wasKinematicField = typeof(XRGrabInteractable).GetField("m_WasKinematic", 
            BindingFlags.NonPublic | BindingFlags.Instance);
            
        //Ensure the wasKinematic field was found
        if (wasKinematicField == null)
        { 
            Debug.LogError("Field m_WasKinematic could not be found through reflection on " +
                           "XRGrabInteractable. Are you using a newer version of the XR Interaction Toolkit?" +
                           "This will likely result in grabables staying kinematic after releasing them.");
            return;
        }
        
        wasKinematicField.SetValue(grabbable, wasKinematic);
    }
}
