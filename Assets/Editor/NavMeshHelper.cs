using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static class NavMeshHelper
{
    [MenuItem("Tools/Bake NavMesh")]
    public static void Bake()
    {
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("NavMesh baked!");
    }

    [MenuItem("Tools/Assign AI Targets")]
    public static void AssignTargets()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("VR Player");
        if (player == null) player = Camera.main.gameObject;

        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        var controllers = GameObject.FindObjectsByType<EnemyAI.StateController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            c.aimTarget = player.transform;
            EditorUtility.SetDirty(c);
        }
        Debug.Log($"Assigned player as target to {controllers.Length} AIs.");
    }
}
