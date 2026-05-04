using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using VRFPSKit;

/// <summary>
/// Editor script that assigns Magazine Detach input actions to M17 pistol's MagazineEjectorInput component.
/// Uses persistent InputActionReference sub-assets already embedded in the .inputactions file.
/// Runs automatically on domain reload.
/// </summary>
[InitializeOnLoad]
public static class FixM17MagazineEjector
{
    static FixM17MagazineEjector()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Fix M17 Magazine Ejector")]
    public static void Apply()
    {
        const string prefabPath       = "Assets/VRFPS Kit/Prefabs/Items/Firearms/M17 9MM.prefab";
        const string inputActionsGuid = "5c5bd7b6b05bb3b4f848a4ba2d36e133";

        const string leftMapName  = "XRI LeftHand Interaction";
        const string rightMapName = "XRI RightHand Interaction";
        const string actionName   = "Magazine Detach";

        // Load input action asset path
        string inputPath = AssetDatabase.GUIDToAssetPath(inputActionsGuid);
        if (string.IsNullOrEmpty(inputPath))
        {
            Debug.LogError("FixM17MagazineEjector: Could not resolve GUID for Input Actions asset.");
            return;
        }

        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
        if (inputAsset == null)
        {
            Debug.LogError("FixM17MagazineEjector: Could not load Input Actions asset at: " + inputPath);
            return;
        }

        // Locate the actions
        var leftAction  = inputAsset.FindActionMap(leftMapName)?.FindAction(actionName);
        var rightAction = inputAsset.FindActionMap(rightMapName)?.FindAction(actionName);
        if (leftAction == null || rightAction == null)
        {
            Debug.LogError($"FixM17MagazineEjector: Could not find '{actionName}' in one or both hand maps.");
            return;
        }

        // Load ALL sub-assets (InputActionReference objects are stored as sub-assets inside the .inputactions file)
        Object[] allSubs = AssetDatabase.LoadAllAssetsAtPath(inputPath);
        InputActionReference leftRef  = null;
        InputActionReference rightRef = null;

        foreach (Object obj in allSubs)
        {
            if (obj is InputActionReference iar)
            {
                if (iar.action?.id == leftAction.id)  leftRef  = iar;
                if (iar.action?.id == rightAction.id) rightRef = iar;
            }
        }

        // If no persistent sub-assets found, create them and add to asset
        if (leftRef == null)
        {
            leftRef = InputActionReference.Create(leftAction);
            leftRef.name = $"{leftMapName}/{actionName}";
            AssetDatabase.AddObjectToAsset(leftRef, inputPath);
            AssetDatabase.SaveAssets();
        }
        if (rightRef == null)
        {
            rightRef = InputActionReference.Create(rightAction);
            rightRef.name = $"{rightMapName}/{actionName}";
            AssetDatabase.AddObjectToAsset(rightRef, inputPath);
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"FixM17MagazineEjector: leftRef={leftRef?.name} rightRef={rightRef?.name}");

        // Edit the M17 prefab
        using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
        var root = scope.prefabContentsRoot;

        // Remove any missing scripts
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        var ejector = root.GetComponentInChildren<MagazineEjectorInput>(true);
        if (ejector == null)
        {
            Debug.LogError("FixM17MagazineEjector: MagazineEjectorInput not found in M17 prefab.");
            return;
        }

        var so        = new SerializedObject(ejector);
        var ejectProp = so.FindProperty("ejectMagazineInput");
        if (ejectProp == null)
        {
            Debug.LogError("FixM17MagazineEjector: Could not find 'ejectMagazineInput' property.");
            return;
        }

        var leftProp  = ejectProp.FindPropertyRelative("leftHandAction");
        var rightProp = ejectProp.FindPropertyRelative("rightHandAction");
        if (leftProp == null || rightProp == null)
        {
            Debug.LogError("FixM17MagazineEjector: Could not find leftHandAction / rightHandAction sub-properties.");
            return;
        }

        leftProp.objectReferenceValue  = leftRef;
        rightProp.objectReferenceValue = rightRef;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("FixM17MagazineEjector: Successfully wired Magazine Detach actions → M17 MagazineEjectorInput!");
    }
}
