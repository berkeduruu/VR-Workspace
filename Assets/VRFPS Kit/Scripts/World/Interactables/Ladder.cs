using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

namespace VRFPSKit
{
    [RequireComponent(typeof(ClimbInteractable))]
    public class Ladder : MonoBehaviour
    {
        [Tooltip("If player is lower than this relative Y level, climb interaction will be ended and player will be teleported to ground")] 
        public float relativeYLevelToEndClimb = -.5f;
        public AudioSource climbSound;

        private ClimbProvider _currentClimber;

        private ClimbInteractable _climbInteractable;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _climbInteractable = GetComponent<ClimbInteractable>();

            //Play sound when selected
            _climbInteractable.selectEntered.AddListener(_ => climbSound?.Play());

            //Track the current climb provider
            _climbInteractable.selectEntered.AddListener(_ => _currentClimber = _climbInteractable.climbProvider);
            _climbInteractable.selectExited.AddListener(_ =>
            {
                //Make sure climb interaction is completely over (we account for two hands possibly grabbing)
                if(!_climbInteractable.isSelected)
                    OnClimbEnd();
            });
        }

        // Update is called once per frame
        private void Update()
        {
            TryEndClimbBecauseTooLowPosition();
        }

        /// <summary>
        /// Will be called when climb interaction is completely over
        /// (no hands holding the ladder anymore)
        /// </summary>
        private void OnClimbEnd()
        {
            //Teleport player to bottom if it is lower than the ladder
            //(Prevents phasing through floor)
            if(GetClimberRelativeYPosition() < 0)
                TeleportClimberToBottom();

            _currentClimber = null;
        }

        private void TryEndClimbBecauseTooLowPosition()
        {
            if (_currentClimber == null) return;
            if (GetClimberRelativeYPosition() > relativeYLevelToEndClimb) return;
        
            TeleportClimberToBottom();
            
            //Cancel ladder interaction
            GetComponent<ClimbInteractable>().interactionManager.CancelInteractableSelection((IXRSelectInteractable)GetComponent<ClimbInteractable>());
        }

        private void TeleportClimberToBottom()
        {
            if (_currentClimber == null)
            {
                Debug.LogWarning("TeleportClimberToBottom(), climber was null!");
                return;
            }
    
            //Teleport player to bottom of ladder
            Vector3 climberPos = _currentClimber.transform.position;
            climberPos.y = transform.position.y;
            _currentClimber.transform.position = climberPos;
        }

        private float GetClimberRelativeYPosition()
        {
            if (_currentClimber == null) return 0;
            Vector3 climberRelativePos = _currentClimber.transform.position - transform.position;

            return climberRelativePos.y;
        }

        // This method is called by the editor to draw gizmos
        private void OnDrawGizmosSelected()
        {
            // Save current matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Apply this object’s local transform matrix
            Gizmos.matrix = transform.localToWorldMatrix;
            
            // Draw a plane at the bottom of the ladder
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, .001f, 1));
            
            // Draw a plane at the minimum Y level for climbing
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(0, relativeYLevelToEndClimb, 0), new Vector3(1, .001f, 1));
            
            // Restore matrix so you don’t mess up other gizmos
            Gizmos.matrix = oldMatrix;
        }
    }
}
