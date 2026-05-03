using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using EnemyAI;

public class ConvertToVR
{
    [MenuItem("Tools/Convert FPS Scene to VR")]
    public static void ConvertScene()
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();

        // 1. Remove old FPS player and conflicting cameras
        GameObject oldPlayer = GameObject.Find("FirstPersonCharacter");
        if (oldPlayer == null) oldPlayer = GameObject.Find("Player");
        if (oldPlayer != null)
        {
            Undo.DestroyObjectImmediate(oldPlayer);
            Debug.Log("Removed old FPS Player.");
        }

        string[] elementsToCopy = new string[] {
            "VR Player", "XR Interaction Manager", "Locomotion System", "XR Device Simulator", "EventSystem", "Global Volume",
            "Pistol", "M4_rifle", "pistol_magazine", "M4_magazine", "AmmoHUD_System"
        };

        // Cleanup existing VR elements if they were already copied (to avoid duplicates)
        foreach (string elementName in elementsToCopy)
        {
            GameObject existing = GameObject.Find(elementName);
            if (existing != null && (existing.scene == currentScene || existing.transform.parent == null))
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }

        // Ensure the Ground has a Teleportation Area - Using string based AddComponent to avoid namespace errors
        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj == null) groundObj = GameObject.Find("Floor");
        if (groundObj != null)
        {
            // We check for TeleportationArea using string to avoid compile-time namespace issues
            bool hasTeleport = groundObj.GetComponent("TeleportationArea") != null;
            if (!hasTeleport)
            {
                // Try to add it by string name
                groundObj.AddComponent(System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Teleportation.TeleportationArea, UnityEngine.XR.Interaction.Toolkit") ?? 
                                     System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea, UnityEngine.XR.Interaction.Toolkit") ??
                                     typeof(BoxCollider)); // Fallback to something safe if not found
                Debug.Log("Attempted to add Teleportation Area to " + groundObj.name);
            }
        }

        // Remove any other Main Cameras that aren't inside VR Player (to avoid conflicts)
        Camera[] allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.name != "Main Camera" || cam.transform.root.name != "VR Player")
            {
                if (cam.tag == "MainCamera")
                {
                    Debug.Log("Disabling conflicting MainCamera: " + cam.gameObject.name);
                    cam.gameObject.tag = "Untagged";
                    cam.enabled = false;
                    // Optionally destroy if it's not the VR camera
                    if (cam.transform.root.name != "VR Player") Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }
        }

        // 2. Open Main scene additively to copy VR elements
        string mainScenePath = "Assets/Scenes/main.unity"; 
        // Try to find main scene
        string[] guids = AssetDatabase.FindAssets("main t:Scene");
        if (guids.Length > 0) mainScenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        Scene mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Additive);

        GameObject vrPlayerRef = null;

        foreach (GameObject rootObj in mainScene.GetRootGameObjects())
        {
            bool shouldCopy = false;
            foreach (string element in elementsToCopy)
            {
                if (rootObj.name == element || rootObj.name.StartsWith(element))
                {
                    shouldCopy = true;
                    break;
                }
            }

            if (shouldCopy)
            {
                // Move object to the current scene
                SceneManager.MoveGameObjectToScene(rootObj, currentScene);
                if (rootObj.name == "VR Player")
                {
                    vrPlayerRef = rootObj;
                    // Reset position a bit
                    rootObj.transform.position = new Vector3(0, 0, 0);
                }
            }
        }

        // Close the main scene
        EditorSceneManager.CloseScene(mainScene, true);

        // 3. Update all enemies to target VR Player
        if (vrPlayerRef != null)
        {
            // Add PlayerHealth if not exists
            if (vrPlayerRef.GetComponent<PlayerHealth>() == null)
            {
                vrPlayerRef.AddComponent<PlayerHealth>();
            }

            StateController[] enemies = UnityEngine.Object.FindObjectsByType<StateController>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                enemy.aimTarget = vrPlayerRef.transform;
                EditorUtility.SetDirty(enemy);
            }

            // Ensure the VR Player's camera is the MainCamera
            Camera vrCam = vrPlayerRef.GetComponentInChildren<Camera>();
            if (vrCam != null)
            {
                vrCam.tag = "MainCamera";
                Debug.Log("VR Camera tagged as MainCamera.");
            }

            Debug.Log("VR Elements copied and enemies updated to target VR Player!");
        }
        else
        {
            Debug.LogError("VR Player could not be found in main scene.");
        }
    }
}
