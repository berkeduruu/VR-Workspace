using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRFPSKit.HandPoses;

namespace VRFPSKit
{
public class VRBodySkin : MonoBehaviour
{
    public HandInteractorPoser leftHandPoser;
    public HandInteractorPoser rightHandPoser;
    public Transform vestTarget;
    public Renderer headRenderer;

    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    [Space] [Space] [Space] [Range(0, 1)] public float turnSmoothness = 0.1f;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    [Space] [Space] [Space] 
    public XRDirectInteractor[] leftHandInteractors;
    public XRDirectInteractor[] rightHandInteractors;

    private bool _isHidingHead;
    
    void Update()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y + headBodyYawOffset;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        //Our hand posers need to know which interactors to use
        if (leftHandPoser != null)
            leftHandPoser.interactors = leftHandInteractors;
        if (rightHandPoser != null)
            rightHandPoser.interactors = rightHandInteractors;
        
        head.Map();
        
        UpdateHandsTracking();
        UpdateHeadHide();
    }

    public void UpdateHandsTracking()
    {
        leftHand.Map();
        rightHand.Map();
        //leftHand.ikTarget.GetComponentInParent<HandIKTrackingDelayFix>()?.CatchUp();
        //rightHand.ikTarget.GetComponentInParent<HandIKTrackingDelayFix>()?.CatchUp();
    }

    private void UpdateHeadHide()
    {
        if(headRenderer != null)
            headRenderer.shadowCastingMode = _isHidingHead ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
    }

    private void Start()
    {
        if (leftHandPoser != null)
            leftHandPoser.interactors = leftHandInteractors;
        if (rightHandPoser != null)
            rightHandPoser.interactors = rightHandInteractors;
    }

    public void SetHideHead(bool hideHead) => _isHidingHead = hideHead;
}

[Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}
}