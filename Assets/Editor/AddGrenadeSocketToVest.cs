using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRFPSKit;

/// <summary>
/// Adds a Grenade Socket to the Magazine Vest prefab, configured like Magazine Socket 2,
/// with XRSpawnWithInteractable to auto-spawn an M67 Grenade.
/// </summary>
[InitializeOnLoad]
public static class AddGrenadeSocketToVest
{
    static AddGrenadeSocketToVest()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Add Grenade Socket to Vest")]
    public static void Apply()
    {
        const string vestPrefabPath    = "Assets/VRFPS Kit/Prefabs/Objects/Player/Vest/Magazine Vest.prefab";
        const string grenadePrefabPath = "Assets/VRFPS Kit/Prefabs/Items/Grenades/M67 Grenade.prefab";
        const string hoverMatPath      = "Assets/VRFPS Kit/Materials/Solid Colors/Hover.mat";
        const string hoverBlockedPath  = "Assets/VRFPS Kit/Materials/Solid Colors/Hover Blocked.mat";
        const string socketName        = "Grenade Socket";

        using var scope = new PrefabUtility.EditPrefabContentsScope(vestPrefabPath);
        var root = scope.prefabContentsRoot;

        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        // Check if already added
        var existing = root.transform.Find(socketName);
        if (existing != null)
        {
            Debug.Log("AddGrenadeSocketToVest: Grenade Socket already exists, skipping.");
            return;
        }

        // Find Magazine Socket 2 to copy its localPosition/localRotation as reference
        var magSocket2 = root.transform.Find("Magazine Socket 2");
        Vector3    localPos = magSocket2 != null
            ? magSocket2.localPosition + new Vector3(0.085f, 0f, 0f)   // shift right by ~8.5cm
            : new Vector3(0.17f, 0f, 0.0164f);
        Quaternion localRot = magSocket2 != null
            ? magSocket2.localRotation
            : Quaternion.Euler(0f, 271.566f, 6.362f);

        // Create the socket GameObject
        var socketGO = new GameObject(socketName);
        socketGO.transform.SetParent(root.transform, false);
        socketGO.transform.localPosition = localPos;
        socketGO.transform.localRotation = localRot;

        // --- SphereCollider (trigger zone) ---
        var col = socketGO.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.12f;

        // --- XRSocketInteractor ---
        var socket = socketGO.AddComponent<XRSocketInteractor>();
        socket.socketActive = true;
        socket.recycleDelayTime = 0f;
        socket.hoverSocketSnapping = false;
        socket.socketSnappingRadius = 0.2f;

        // Hover meshes
        var hoverMat       = AssetDatabase.LoadAssetAtPath<Material>(hoverMatPath);
        var hoverBlockMat  = AssetDatabase.LoadAssetAtPath<Material>(hoverBlockedPath);
        if (hoverMat != null)       socket.interactableHoverMeshMaterial     = hoverMat;
        if (hoverBlockMat != null)  socket.interactableCantHoverMeshMaterial  = hoverBlockMat;

        // Interaction layer: copy from Magazine Socket 2 if present, else use "Interactable" (layer 3 = value 8)
        if (magSocket2 != null)
        {
            var refSocket = magSocket2.GetComponent<XRSocketInteractor>();
            if (refSocket != null)
                socket.interactionLayers = refSocket.interactionLayers;
        }

        // --- XRSocketTrackingModeFix ---
        socketGO.AddComponent<XRSocketTrackingModeFix>();

        // --- XRSocketOnlyHeldInteractablesFilter (only accept items that are held) ---
        var onlyHeld = magSocket2?.GetComponent<XRSocketOnlyHeldInteractablesFilter>();
        if (onlyHeld != null)
            socketGO.AddComponent<XRSocketOnlyHeldInteractablesFilter>();

        // --- XRSocketDelayedTrackingFix ---
        var delayedFix = magSocket2?.GetComponent<XRSocketDelayedTrackingFix>();
        if (delayedFix != null)
            socketGO.AddComponent<XRSocketDelayedTrackingFix>();

        // --- XRSpawnWithInteractable: auto-spawn M67 grenade ---
        var spawn = socketGO.AddComponent<XRSpawnWithInteractable>();
        var grenadePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(grenadePrefabPath);
        if (grenadePrefab == null)
        {
            Debug.LogError("AddGrenadeSocketToVest: M67 Grenade prefab not found at: " + grenadePrefabPath);
            return;
        }

        var spawnSO = new SerializedObject(spawn);
        var prefabProp    = spawnSO.FindProperty("selectedSpawnPrefab");
        var respawnProp   = spawnSO.FindProperty("respawnOnDeath");
        if (prefabProp  != null) prefabProp.objectReferenceValue  = grenadePrefab;
        if (respawnProp != null) respawnProp.boolValue = true;
        spawnSO.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"AddGrenadeSocketToVest: Grenade Socket added to Magazine Vest at localPos={localPos}");
    }
}
