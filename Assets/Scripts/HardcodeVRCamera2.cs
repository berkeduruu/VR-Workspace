using UnityEngine;
using UnityEngine.XR;

public class HardcodeVRCamera2 : MonoBehaviour
{
    private InputDevice centerEye;

    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    void Update()
    {
        UpdateCameraPose();
    }

    void OnBeforeRender()
    {
        UpdateCameraPose();
    }

    void UpdateCameraPose()
    {
        if (!centerEye.isValid)
        {
            centerEye = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        }

        if (centerEye.isValid)
        {
            if (centerEye.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 position))
            {
                transform.localPosition = position;
            }
            if (centerEye.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion rotation))
            {
                transform.localRotation = rotation;
            }
        }
    }
}
