using UnityEngine;
using UnityEditor;

public static class PhysicsFixer
{
    [MenuItem("AI_Fix/Fix Physics and Layers")]
    public static void Fix()
    {
        // 1. Ensure Layer 7 (Bullet) collides with Layer 12 (Enemy/ChildInteractable)
        Physics.IgnoreLayerCollision(7, 12, false);
        Physics.IgnoreLayerCollision(7, 0, false); // Default
        Physics.IgnoreLayerCollision(7, 11, false); // Interactable
        
        Debug.Log("Physics Matrix Updated: Layer 7 (Bullet) now collides with 0, 11, 12.");

        // 2. Fix Bullet Prefab Layer
        string bulletPath = "Assets/VRFPS Kit/Prefabs/Objects/Bullet.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath);
        if (bulletPrefab != null)
        {
            bulletPrefab.layer = 7; // Bullet Layer
            EditorUtility.SetDirty(bulletPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Bullet Prefab Layer set to 7 (Bullet).");
        }
        else
        {
            Debug.LogError("Bullet Prefab not found at " + bulletPath);
        }

        // 3. Fix Enemy Layers (Ensure they are on 12)
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            e.layer = 12;
            // Also set children (bones) to layer 12
            SetLayerRecursive(e, 12);
            Debug.Log($"Set {e.name} and children to Layer 12.");
        }
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}
