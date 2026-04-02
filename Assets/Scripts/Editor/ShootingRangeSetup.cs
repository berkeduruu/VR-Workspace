using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ShootingRangeSetup
{
    private static readonly string[] ObjectsToDelete = {
        "VR Player",
        "XR Interaction Manager",
        "XR Device Simulator",
        "GunAssemblyStation",
        "Input Test",
        "Sphere",
        "Table 2",
        "CompletedGun",
        "Weapon_02"
    };

    [MenuItem("Tools/Shooting Range/Setup Shooting Range")]
    public static void SetupShootingRange()
    {
        // ========== 1. DELETE OLD OBJECTS ==========
        foreach (string name in ObjectsToDelete)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
                Debug.Log($"[ShootingRange] Deleted: {name}");
            }
        }

        // ========== 2. ADD HVR INFRASTRUCTURE ==========
        
        // HVRGlobal (required system manager)
        GameObject hvrGlobalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/HurricaneVR/Framework/Prefabs/HVRGlobal.prefab");
        if (hvrGlobalPrefab != null && GameObject.Find("HVRGlobal") == null)
        {
            PrefabUtility.InstantiatePrefab(hvrGlobalPrefab);
            Debug.Log("[ShootingRange] Added HVRGlobal");
        }

        // TechDemoXRRig (player rig with hands, grab, teleport)
        GameObject rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/HurricaneVR/TechDemo/Prefabs/TechDemoXRRig.prefab");
        if (rigPrefab != null && GameObject.Find("TechDemoXRRig") == null)
        {
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            rig.transform.position = new Vector3(0, 0, 0);
            Debug.Log("[ShootingRange] Added TechDemoXRRig at origin");
        }

        // UIManager (event system for HVR)
        GameObject uiManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/HurricaneVR/TechDemo/Prefabs/UIManager.prefab");
        if (uiManagerPrefab != null && GameObject.Find("UIManager") == null)
        {
            PrefabUtility.InstantiatePrefab(uiManagerPrefab);
            Debug.Log("[ShootingRange] Added UIManager");
        }

        // ========== 3. ENSURE GROUND ==========
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            // Dark floor material
            var renderer = ground.GetComponent<Renderer>();
            renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }

        // ========== 4. DIRECTIONAL LIGHT ==========
        if (Object.FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        // ========== 5. WEAPON TABLE ==========
        string tablePath = "Assets/HurricaneVR/TechDemo/Assets/Interactables/Props/table.prefab";
        GameObject tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tablePath);
        
        GameObject weaponTable = null;
        GameObject ammoTable = null;

        if (tablePrefab != null)
        {
            // Weapon table - in front of player
            weaponTable = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
            weaponTable.name = "WeaponTable";
            weaponTable.transform.position = new Vector3(0, 0, 1.5f);
            weaponTable.transform.rotation = Quaternion.identity;
            Debug.Log("[ShootingRange] Added WeaponTable");

            // Ammo table - to the right
            ammoTable = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
            ammoTable.name = "AmmoTable";
            ammoTable.transform.position = new Vector3(1.5f, 0, 1.5f);
            ammoTable.transform.rotation = Quaternion.identity;
            Debug.Log("[ShootingRange] Added AmmoTable");
        }

        // ========== 6. PLACE WEAPONS ON TABLE ==========
        float tableHeight = 0.9f; // approximate table surface height

        // Pistol
        PlacePrefab(
            "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/HVR_pistol.prefab",
            "HVR_pistol",
            new Vector3(-0.3f, tableHeight + 0.05f, 1.5f),
            Quaternion.Euler(0, 90, 0)
        );

        // SMG
        PlacePrefab(
            "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/HVR_smg.prefab",
            "HVR_smg",
            new Vector3(0f, tableHeight + 0.05f, 1.5f),
            Quaternion.Euler(0, 90, 0)
        );

        // Shotgun
        PlacePrefab(
            "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/HVR_shotgun.prefab",
            "HVR_shotgun",
            new Vector3(0.3f, tableHeight + 0.05f, 1.5f),
            Quaternion.Euler(0, 90, 0)
        );

        // Combat Knife
        PlacePrefab(
            "Assets/HurricaneVR/TechDemo/Assets/Interactables/MeleeWeapons/CombatKnife.prefab",
            "CombatKnife",
            new Vector3(0.55f, tableHeight + 0.05f, 1.5f),
            Quaternion.Euler(0, 90, 0)
        );

        // ========== 7. PLACE AMMO ON SECOND TABLE ==========
        float ammoTableX = 1.5f;
        float ammoZ = 1.5f;

        // Pistol magazines (5x)
        for (int i = 0; i < 5; i++)
        {
            PlacePrefab(
                "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/HVR_pistol_magazine.prefab",
                $"HVR_pistol_magazine ({i})",
                new Vector3(ammoTableX - 0.25f, tableHeight + 0.05f + i * 0.03f, ammoZ - 0.15f + i * 0.07f),
                Quaternion.identity
            );
        }

        // SMG magazines (5x)
        for (int i = 0; i < 5; i++)
        {
            PlacePrefab(
                "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/HVR_smg_magazine.prefab",
                $"HVR_smg_magazine ({i})",
                new Vector3(ammoTableX, tableHeight + 0.05f + i * 0.03f, ammoZ - 0.15f + i * 0.07f),
                Quaternion.identity
            );
        }

        // Shotgun shells (5x)
        for (int i = 0; i < 5; i++)
        {
            PlacePrefab(
                "Assets/HurricaneVR/TechDemo/Assets/Interactables/Guns/ShotgunShell.prefab",
                $"ShotgunShell ({i})",
                new Vector3(ammoTableX + 0.25f, tableHeight + 0.05f + i * 0.02f, ammoZ - 0.1f + i * 0.05f),
                Quaternion.identity
            );
        }

        // ========== 8. SHOOTING TARGETS ==========
        string targetPath = "Assets/HurricaneVR/TechDemo/Assets/Interactables/Props/basic_target.prefab";

        // Close target (5m)
        PlacePrefab(targetPath, "Target_5m",
            new Vector3(-1f, 0, 6.5f),
            Quaternion.Euler(0, 180, 0)
        );

        // Medium target (10m)
        PlacePrefab(targetPath, "Target_10m",
            new Vector3(0f, 0, 11.5f),
            Quaternion.Euler(0, 180, 0)
        );

        // Far target (15m)
        PlacePrefab(targetPath, "Target_15m",
            new Vector3(1f, 0, 16.5f),
            Quaternion.Euler(0, 180, 0)
        );

        // ========== 9. KNIFE TARGET (DUMMY) ==========
        PlacePrefab(
            "Assets/HurricaneVR/TechDemo/Assets/Interactables/Props/Dummy/Dummy.prefab",
            "KnifeDummy",
            new Vector3(2.5f, 0, 4f),
            Quaternion.Euler(0, 180, 0)
        );

        // ========== 10. ORGANIZE IN PARENT ==========
        // Create a parent container for cleanliness
        GameObject shootingRange = new GameObject("ShootingRange");
        shootingRange.transform.position = Vector3.zero;

        // Parent weapons
        ParentIfExists("HVR_pistol", shootingRange);
        ParentIfExists("HVR_smg", shootingRange);
        ParentIfExists("HVR_shotgun", shootingRange);
        ParentIfExists("CombatKnife", shootingRange);

        // Parent tables
        ParentIfExists("WeaponTable", shootingRange);
        ParentIfExists("AmmoTable", shootingRange);

        // Parent ammo
        for (int i = 0; i < 5; i++)
        {
            ParentIfExists($"HVR_pistol_magazine ({i})", shootingRange);
            ParentIfExists($"HVR_smg_magazine ({i})", shootingRange);
            ParentIfExists($"ShotgunShell ({i})", shootingRange);
        }

        // Parent targets
        ParentIfExists("Target_5m", shootingRange);
        ParentIfExists("Target_10m", shootingRange);
        ParentIfExists("Target_15m", shootingRange);
        ParentIfExists("KnifeDummy", shootingRange);

        // ========== SAVE ==========
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("========================================");
        Debug.Log("🎯 SHOOTING RANGE SETUP COMPLETE!");
        Debug.Log("  3 Guns: Pistol, SMG, Shotgun");
        Debug.Log("  1 Knife: CombatKnife (throwable)");
        Debug.Log("  3 Targets: 5m, 10m, 15m");
        Debug.Log("  1 Dummy: For knife throwing");
        Debug.Log("  Ammo: 5x each (pistol mag, SMG mag, shotgun shell)");
        Debug.Log("========================================");
    }

    private static GameObject PlacePrefab(string path, string name, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[ShootingRange] Prefab not found: {path}");
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        Debug.Log($"[ShootingRange] Placed: {name} at {position}");
        return instance;
    }

    private static void ParentIfExists(string name, GameObject parent)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            obj.transform.SetParent(parent.transform, true);
        }
    }
}
