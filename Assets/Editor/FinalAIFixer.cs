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
        Transform targetTransform = null;
        if (player != null)
        {
            player.tag = "Player";
            // Target the Main Camera if possible for accurate head tracking
            var cam = player.GetComponentInChildren<Camera>();
            targetTransform = cam != null ? cam.transform : player.transform;
            Debug.Log("Fixed VR Player Tag. Targeting: " + targetTransform.name);
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
            // Set Player (Camera) as aim target
            if (targetTransform != null) c.aimTarget = targetTransform;
            
            // Add Debugger
            if (c.GetComponent<AIDebugger>() == null) c.gameObject.AddComponent<AIDebugger>();
            
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
