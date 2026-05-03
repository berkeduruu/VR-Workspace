using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// A socket interaction filter that ensures the XRSocketInteractor only accepts interactables
    /// that have been previously held by the player. 
    /// 
    /// This script tracks interactables that are currently selected by a hand (XRBaseInputInteractor)
    /// and only allows the socket to interact with them. Hovering and selection of unheld or idle
    /// items is ignored, preventing sockets from automatically "vacuuming" objects in the scene.
    /// 
    /// The script maintains a temporary list of hovered held interactables, updates it on hover
    /// enter/exit, and provides delayed cleanup to allow smooth interaction transitions.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))] [DisallowMultipleComponent]
    public class XRSocketOnlyHeldInteractablesFilter : MonoBehaviour
    {
        private List<IXRInteractable> _hoveredHeldInteractables = new ();
        
        private XRSocketInteractor _socket;
        
        /// <summary>
        /// An Interaction Filter that will only accept Interactables that were previously held by the player
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="interactable"></param>
        /// <returns></returns>
        private bool WasInteractableHeldByPlayer(IXRInteractor interactor, IXRInteractable interactable)
        {
            //Skip this filter if already attached
            if (((IXRSelectInteractor) interactor).IsSelecting((IXRSelectInteractable) interactable)) return true;
            //Only accept held Interactables
            //Held interactables need to be stored in a list since they need to be dropped in order to be selected by this socket
            if (!_hoveredHeldInteractables.Contains(interactable)) return false;
            
            return true;
        }
        
        private bool IsInteractableHeld(IXRSelectInteractable interactable)
        {
            //Only accept selected Interactables
            //Make sure the selector is a hand
            return interactable.isSelected && interactable.interactorsSelecting[0] is XRBaseInputInteractor;
        }

        // Start is called before the first frame update
        void Awake()
        {
            _socket = GetComponent<XRSocketInteractor>();

            _socket.hoverFilters.Add(new XRHoverFilterDelegate((_, interactable) => IsInteractableHeld((IXRSelectInteractable) interactable)));
            
            _socket.hoverEntered.AddListener(OnHoverEnter);
            _socket.hoverExited.AddListener(args => UntrackHeldInteractable(args.interactableObject));
            _socket.selectEntered.AddListener(args => UntrackHeldInteractable(args.interactableObject));
        }
        
        private void OnHoverEnter(HoverEnterEventArgs args)
        {
            if (IsInteractableHeld((IXRSelectInteractable)args.interactableObject))
                _hoveredHeldInteractables.Add(args.interactableObject);
        }
        
        private async void UntrackHeldInteractable(IXRInteractable interactable)
        {
            await Task.Delay(100);
            
            _hoveredHeldInteractables.Remove(interactable);
        }
    }
}