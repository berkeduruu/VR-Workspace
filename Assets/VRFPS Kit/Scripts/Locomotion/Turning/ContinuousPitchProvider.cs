using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace VRFPSKit.Locomotion.Turning
{
    public class ContinuousPitchProvider : MonoBehaviour
    {
        public InputActionProperty rightHandTurnAction;
        public float turnSpeed = 60f;

        [Header("References")]
        public Transform cameraOffset;

        private float currentPitch = 0f;

        void Start()
        {
            if (cameraOffset == null)
            {
                XROrigin xrOrigin = GetComponent<XROrigin>();
                if (xrOrigin != null)
                {
                    cameraOffset = xrOrigin.CameraFloorOffsetObject.transform;
                }
            }
        }

        void Update()
        {
            if (cameraOffset == null) return;

            Vector2 input = rightHandTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
            if (Mathf.Abs(input.y) > 0.1f)
            {
                // input.y > 0 means joystick pushed forward/up. Usually this means look up, which is a negative pitch in Unity.
                float pitchAmount = input.y * turnSpeed * Time.deltaTime;
                currentPitch -= pitchAmount; 

                // Keep currentPitch within 0 to 360 range for cleanliness, though Euler handles wrapping automatically
                if (currentPitch > 360f) currentPitch -= 360f;
                if (currentPitch < -360f) currentPitch += 360f;

                Vector3 currentEuler = cameraOffset.localEulerAngles;
                cameraOffset.localRotation = Quaternion.Euler(currentPitch, currentEuler.y, currentEuler.z);
            }
        }
    }
}
