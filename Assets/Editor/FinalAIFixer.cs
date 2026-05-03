using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static class FinalAIFixer
{
    [MenuItem("AI_Fix/ULTIMATE FIX")]
    public static void FixEverything()
    {
        // 1. Fix Player
        var player = GameObject.Find("VR Player");
        if (player != null)
        {
            player.tag = "Player";
            Debug.Log("Fixed VR Player Tag.");
        }
        else
        {
            Debug.LogError("VR Player not found!");
        }

        // 2. Bake NavMesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("NavMesh Baked.");

        // 3. Fix Enemies
        var controllers = GameObject.FindObjectsByType<EnemyAI.StateController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            // Set Player as aim target
            if (player != null) c.aimTarget = player.transform;
            
            // Ensure they have a NavMeshAgent and snap it
            var agent = c.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(c.transform.position, out hit, 5.0f, NavMesh.AllAreas))
                {
                    c.transform.position = hit.position;
                    agent.enabled = false;
                    agent.enabled = true;
                    Debug.Log($"Snapped {c.name} to NavMesh.");
                }
            }
            
            EditorUtility.SetDirty(c);
        }

        Debug.Log("ULTIMATE FIX COMPLETED. Please press Play.");
    }
}
