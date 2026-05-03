using UnityEngine;
using EnemyAI;

public class AIDebugger : MonoBehaviour
{
    StateController controller;

    void Start()
    {
        controller = GetComponent<StateController>();
    }

    void Update()
    {
        if (controller == null) return;

        if (Time.frameCount % 60 == 0) // Every second
        {
            float dist = Vector3.Distance(transform.position, controller.aimTarget.position);
            bool blocked = controller.BlockedSight();
            Debug.Log($"[AI Debug] {name} | Target: {controller.aimTarget.name} | Pos: {controller.aimTarget.position} | Dist: {dist} | InSight: {controller.targetInSight} | Blocked: {blocked}");
        }
    }
}
