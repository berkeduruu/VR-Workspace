using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// XR Input System binding'lerinin bozulmasi veya calismamasi durumunda
/// kameranin HMD (CenterEye) tarafindan donanim seviyesinde takip edilmesini garanti altina alan script.
/// </summary>
public class HardcodeVRCamera : MonoBehaviour
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
