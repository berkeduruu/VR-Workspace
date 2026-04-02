using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class SimulatorOffsetFixer : MonoBehaviour
{
    [Header("Offset Settings")]
    [Tooltip("Starting distance from camera offset root")]
    [SerializeField] private float defaultZOffset = -0.25f;
    [SerializeField] private float minZOffset = -0.8f;
    [SerializeField] private float maxZOffset = 0.4f;
    [SerializeField] private float offsetSpeed = 1.0f;

    [Header("Manual Assignments (Optional)")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    // Current offsets for each hand
    private float leftZOffset;
    private float rightZOffset;

    // Container references
    private Transform containerLeft;
    private Transform containerRight;

    private void Awake()
    {
        // Initialize offsets with default value
        leftZOffset = defaultZOffset;
        rightZOffset = defaultZOffset;

        Invoke(nameof(SetupHierarchyOffset), 0.1f);
    }

    private void SetupHierarchyOffset()
    {
        if (leftHand == null || rightHand == null)
        {
            XROrigin xrOrigin = FindAnyObjectByType<XROrigin>();
            if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
            {
                Transform offsetRoot = xrOrigin.CameraFloorOffsetObject.transform;
                if (leftHand == null) leftHand = offsetRoot.Find("Left_Hand");
                if (rightHand == null) rightHand = offsetRoot.Find("Right_Hand");
            }
        }

        if (leftHand != null) containerLeft = CreateOffsetParent(leftHand, "Left_Hand_Simulator_Offset", leftZOffset);
        if (rightHand != null) containerRight = CreateOffsetParent(rightHand, "Right_Hand_Simulator_Offset", rightZOffset);
    }

    private Transform CreateOffsetParent(Transform hand, string name, float initialZ)
    {
        Transform originalParent = hand.parent;
        GameObject container = new GameObject(name);
        container.transform.SetParent(originalParent, false);
        container.transform.localPosition = new Vector3(0, 0, initialZ);
        container.transform.localRotation = Quaternion.identity;
        hand.SetParent(container.transform, false);
        return container.transform;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Determine which hand is selected
        bool leftSelected = Keyboard.current.leftShiftKey.isPressed;
        bool rightSelected = Keyboard.current.spaceKey.isPressed;

        // 2. Read Input
        float moveInput = 0f;
        if (Keyboard.current.rightBracketKey.isPressed || Keyboard.current.numpadPlusKey.isPressed) moveInput += 1f;
        if (Keyboard.current.leftBracketKey.isPressed || Keyboard.current.numpadMinusKey.isPressed) moveInput -= 1f;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float delta = moveInput * Time.deltaTime * offsetSpeed;

            // 3. Apply to selected hand(s)
            if (leftSelected)
            {
                leftZOffset = Mathf.Clamp(leftZOffset + delta, minZOffset, maxZOffset);
                ApplyOffset(containerLeft, leftZOffset);
            }
            
            if (rightSelected)
            {
                rightZOffset = Mathf.Clamp(rightZOffset + delta, minZOffset, maxZOffset);
                ApplyOffset(containerRight, rightZOffset);
            }

            // If neither is specifically held, maybe the user wants to move both?
            // (Optional: remove this if you only want it to work when a key is held)
            if (!leftSelected && !rightSelected)
            {
                leftZOffset = Mathf.Clamp(leftZOffset + delta, minZOffset, maxZOffset);
                rightZOffset = Mathf.Clamp(rightZOffset + delta, minZOffset, maxZOffset);
                ApplyOffset(containerLeft, leftZOffset);
                ApplyOffset(containerRight, rightZOffset);
            }
        }
    }

    private void ApplyOffset(Transform container, float zOffset)
    {
        if (container != null)
        {
            container.localPosition = new Vector3(0, 0, zOffset);
        }
    }
}
