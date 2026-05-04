using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AddLaserToWeapons
{
    [MenuItem("Tools/Add Lasers To Weapons")]
    public static void AddLasers()
    {
        string[] weaponPaths = new string[]
        {
            "Assets/VRFPS Kit/Prefabs/Items/Firearms/M17 9MM.prefab",
            "Assets/VRFPS Kit/Prefabs/Items/Firearms/M16.prefab"
        };
        string laserPath = "Assets/VRFPS Kit/Prefabs/Items/Attachments/HS LS321g.prefab";

        GameObject laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(laserPath);
        if (laserPrefab == null)
        {
            Debug.LogError("Could not find laser prefab at " + laserPath);
            return;
        }

        foreach (string path in weaponPaths)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject prefabRoot = editingScope.prefabContentsRoot;
                
                Transform underbarrelRail = FindChildRecursive(prefabRoot.transform, "Underbarrel Rail");
                
                if (underbarrelRail != null)
                {
                    XRSocketInteractor socket = underbarrelRail.GetComponent<XRSocketInteractor>();
                    if (socket != null && socket.startingSelectedInteractable == null)
                    {
                        GameObject laserInstance = (GameObject)PrefabUtility.InstantiatePrefab(laserPrefab, underbarrelRail);
                        laserInstance.transform.localPosition = Vector3.zero;
                        laserInstance.transform.localRotation = Quaternion.identity;

                        XRGrabInteractable grabInteractable = laserInstance.GetComponent<XRGrabInteractable>();
                        if (grabInteractable != null)
                        {
                            socket.startingSelectedInteractable = grabInteractable;
                        }
                        Debug.Log("Successfully attached laser to socket in " + path);
                    }
                }
                else
                {
                    // No Underbarrel Rail, let's just extract the Laser Ray part and put it on bullet spawn
                    Transform bulletSpawn = FindChildRecursive(prefabRoot.transform, "bullet spawn");
                    if (bulletSpawn != null)
                    {
                        if (FindChildRecursive(bulletSpawn, "Laser Ray") != null)
                        {
                            Debug.Log("Laser already attached to bullet spawn in " + path);
                            continue;
                        }

                        // Use normal Instantiate to avoid prefab instance restrictions
                        GameObject fullLaser = Object.Instantiate(laserPrefab, bulletSpawn);
                        Transform laserRay = fullLaser.transform.Find("Laser Ray");
                        
                        if (laserRay != null)
                        {
                            laserRay.SetParent(bulletSpawn);
                            laserRay.localPosition = Vector3.zero;
                            laserRay.localRotation = Quaternion.identity;
                            Object.DestroyImmediate(fullLaser);
                            Debug.Log("Successfully added bare laser to bullet spawn in " + path);
                        }
                        else
                        {
                            Object.DestroyImmediate(fullLaser);
                        }
                    }
                }
            }
        }
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
