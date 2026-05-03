using UnityEngine;
using UnityEditor;
using EnemyAI;

public class SpawnEnemies
{
    [MenuItem("Tools/Spawn Enemies")]
    public static void Spawn()
    {
        GameObject player = GameObject.Find("VR Player");
        if (player == null) {
            Debug.LogError("Could not find VR Player");
            return;
        }

        Transform target = player.transform; // Assuming the target is the VR Player

        string[] prefabPaths = new string[] {
            "Assets/EnemyAI/Examples/Prefabs/enemy_ak.prefab",
            "Assets/EnemyAI/Examples/Prefabs/enemy_pistol.prefab",
            "Assets/EnemyAI/Examples/Prefabs/enemy_rifle.prefab"
        };

        Vector3[] positions = new Vector3[] {
            new Vector3(-3, 0, 6),
            new Vector3(3, 0, 6),
            new Vector3(0, 0, 10)
        };

        for(int i=0; i<prefabPaths.Length; i++) {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
            if (prefab != null) {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = positions[i];
                instance.transform.rotation = Quaternion.Euler(0, 180, 0);
                
                StateController controller = instance.GetComponent<StateController>();
                if (controller != null) {
                    controller.aimTarget = target;
                }
                
                EnemyHealth health = instance.GetComponent<EnemyHealth>();
                if (health != null) {
                    health.health = 50f; // Make them not too hard
                }
                
                Debug.Log($"Spawned {prefab.name} at {positions[i]}");
            } else {
                Debug.LogError($"Could not find prefab at {prefabPaths[i]}");
            }
        }
    }
}