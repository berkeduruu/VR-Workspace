using UnityEngine;

public class SimulatorOffsetFixer : MonoBehaviour
{
    void Start()
    {
        // Give the simulator a split second to initialize
        Invoke(nameof(ApplyFix), 0.1f);
    }

    private void ApplyFix()
    {
        // Try direct children first (often named LeftHand/RightHand or similar in current scene)
        Transform left = transform.Find("Left_Hand");
        Transform right = transform.Find("Right_Hand");

        // If not children, search in descendants
        if (left == null) left = FindChildRecursive(transform, "Left");
        if (right == null) right = FindChildRecursive(transform, "Right");

        if (left != null) left.localPosition = new Vector3(-0.2f, -0.3f, 0.15f);
        if (right != null) right.localPosition = new Vector3(0.2f, -0.3f, 0.15f);

        Debug.Log("SimulatorOffsetFixer: Hand offsets adjusted for easy reach.");
    }

    private Transform FindChildRecursive(Transform parent, string nameContains)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(nameContains)) return child;
            Transform found = FindChildRecursive(child, nameContains);
            if (found != null) return found;
        }
        return null;
    }
}
