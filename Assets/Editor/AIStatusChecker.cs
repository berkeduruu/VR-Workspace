using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static class AIStatusChecker
{
    [MenuItem("Tools/Check AI and NavMesh")]
    public static void Check()
    {
        var agents = GameObject.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
        Debug.Log($"Found {agents.Length} agents.");

        foreach (var agent in agents)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                Debug.Log($"{agent.name} is ON NavMesh (dist: {Vector3.Distance(agent.transform.position, hit.position)})");
            }
            else
            {
                Debug.LogError($"{agent.name} is NOT on NavMesh!");
            }

            var controller = agent.GetComponent<EnemyAI.StateController>();
            if (controller != null)
            {
                if (controller.aimTarget == null)
                {
                    Debug.LogWarning($"{agent.name} has NULL aimTarget.");
                    var player = GameObject.Find("VR Player");
                    if (player != null)
                    {
                        controller.aimTarget = player.transform;
                        EditorUtility.SetDirty(controller);
                        Debug.Log($"Fixed {agent.name} aimTarget.");
                    }
                }
            }
        }

        // Check if NavMesh exists at all
        NavMeshHit globalHit;
        if (NavMesh.SamplePosition(Vector3.zero, out globalHit, 1000f, NavMesh.AllAreas))
        {
            Debug.Log("NavMesh exists in the scene.");
        }
        else
        {
            Debug.LogError("NO NavMesh found in the scene! Please bake it.");
        }
    }
}
