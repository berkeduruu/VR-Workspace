using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static class UniqueAIWatcher
{
    [MenuItem("AI_Fix/Fix All AI")]
    public static void FixAI()
    {
        // 1. Bake NavMesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("UniqueAIWatcher: NavMesh Baked.");

        // 2. Find Player
        var player = GameObject.Find("VR Player");
        if (player == null) player = GameObject.FindWithTag("Player");
        if (player == null) player = Camera.main != null ? Camera.main.gameObject : null;

        if (player == null)
        {
            Debug.LogError("UniqueAIWatcher: Player NOT found!");
            return;
        }

        // 3. Fix Controllers
        var controllers = GameObject.FindObjectsByType<EnemyAI.StateController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            c.aimTarget = player.transform;
            EditorUtility.SetDirty(c);
            Debug.Log($"UniqueAIWatcher: Assigned target to {c.name}");
        }

        // 4. Ensure AIs are on NavMesh
        foreach (var agent in GameObject.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.transform.position = hit.position;
                agent.enabled = false;
                agent.enabled = true; // Reset agent to snap to NavMesh
                Debug.Log($"UniqueAIWatcher: Snapped {agent.name} to NavMesh.");
            }
            else
            {
                Debug.LogError($"UniqueAIWatcher: {agent.name} is TOO FAR from NavMesh!");
            }
        }
        
        AssetDatabase.SaveAssets();
    }
}
