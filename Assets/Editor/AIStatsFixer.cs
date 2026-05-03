using UnityEngine;
using UnityEditor;
using EnemyAI;

public static class AIStatsFixer
{
    [MenuItem("AI_Fix/Fix AI Stats and Masks")]
    public static void Fix()
    {
        string statsPath = "Assets/EnemyAI/EnemyStats/EnemyStats.asset";
        GeneralStats stats = AssetDatabase.LoadAssetAtPath<GeneralStats>(statsPath);
        if (stats != null)
        {
            // 1. Target Mask: Should include Player (Layer 6)
            // Bit 6 = 1 << 6 = 64
            stats.targetMask = (1 << 6); 
            
            // 2. Obstacle Mask: Should include Default (Layer 0)
            // Bit 0 = 1
            stats.obstacleMask = (1 << 0);
            
            // 3. Cover Mask: Usually includes obstacles or specific cover layers
            // I'll set it to something reasonable if it was broken
            // stats.coverMask = ...
            
            EditorUtility.SetDirty(stats);
            AssetDatabase.SaveAssets();
            Debug.Log("AI EnemyStats updated: TargetMask set to Player (Layer 6), ObstacleMask set to Default.");
        }
        else
        {
            Debug.LogError("EnemyStats not found at " + statsPath);
        }

        // Also ensure all enemies are using this stats object
        var controllers = GameObject.FindObjectsByType<StateController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            c.generalStats = stats;
            c.viewRadius = 50f; // Ensure they can see far
            c.viewAngle = 180f; // Wide view
            EditorUtility.SetDirty(c);
        }
        Debug.Log("All enemies updated with correct stats and FOV.");
    }
}
