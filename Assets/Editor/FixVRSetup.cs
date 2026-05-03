using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fixes the VR hand setup so XR Device Simulator can drive LeftHand and RightHand.
/// Menu: Tools > Fix VR Setup (XR Simulator)
/// </summary>
public static class FixVRSetup
{
    private const string ACTIONS_PATH = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions";

    [MenuItem("Tools/Fix VR Setup (XR Simulator)")]
    public static void Fix()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ACTIONS_PATH);
        if (asset == null)
        {
            Debug.LogError("[FixVR] XRI Default Input Actions not found at: " + ACTIONS_PATH);
            return;
        }

        // Load all sub-assets (InputActionReferences) once
        var allRefs = AssetDatabase.LoadAllAssetsAtPath(ACTIONS_PATH).OfType<InputActionReference>().ToList();

        EnsureInputActionManager(asset);

        // Player Move/Turn
        var player = Object.FindAnyObjectByType<VRPlayerController>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            BindReference(so, "moveAction", allRefs, "XRI Left Locomotion/Move");
            BindReference(so, "turnAction", allRefs, "XRI Right Locomotion/Turn");
            so.ApplyModifiedProperties();
            Debug.Log("[FixVR] VRPlayerController wired for XR Locomotion.");
        }

        // Weapon Reload
        var weapon = Object.FindAnyObjectByType<VRWeaponKeyboard>();
        if (weapon != null)
        {
            var so = new SerializedObject(weapon);
            BindReference(so, "reloadAction", allRefs, "XRI Right Interaction/UI Press");
            so.ApplyModifiedProperties();
            Debug.Log("[FixVR] VRWeaponKeyboard wired for XR Reload.");
        }

        // Controllers
        var controllers = Object.FindObjectsByType<ActionBasedController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            string side = c.name.ToLower().Contains("left") ? "Left" : "Right";
            var so = new SerializedObject(c);
            
            BindProperty(so, "m_PositionAction", allRefs, $"XRI {side}/Position");
            BindProperty(so, "m_RotationAction", allRefs, $"XRI {side}/Rotation");
            BindProperty(so, "m_IsTrackedAction", allRefs, $"XRI {side}/Is Tracked");
            BindProperty(so, "m_TrackingStateAction", allRefs, $"XRI {side}/Tracking State");
            
            BindProperty(so, "m_SelectAction", allRefs, $"XRI {side} Interaction/Select");
            BindProperty(so, "m_SelectActionValue", allRefs, $"XRI {side} Interaction/Select Value");
            BindProperty(so, "m_ActivateAction", allRefs, $"XRI {side} Interaction/Activate");
            BindProperty(so, "m_ActivateActionValue", allRefs, $"XRI {side} Interaction/Activate Value");
            BindProperty(so, "m_UIPressAction", allRefs, $"XRI {side} Interaction/UI Press");
            
            so.ApplyModifiedProperties();
            Debug.Log($"[FixVR] {c.name} wired OK.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("<b>[FixVR]</b> Done!");
    }

    static void BindReference(SerializedObject so, string propName, List<InputActionReference> allRefs, string actionPath)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) return;

        // actionPath is like "XRI Left Locomotion/Move"
        // Find reference where the name ends with the action name or matches exactly
        string actionName = actionPath.Split('/').Last();
        var actionRef = allRefs.FirstOrDefault(r => r.name == actionPath || r.name == actionName || r.name.EndsWith("/" + actionName));

        if (actionRef != null)
        {
            prop.objectReferenceValue = actionRef;
        }
        else
        {
            Debug.LogWarning($"[FixVR] Could not find reference for {actionPath}");
        }
    }

    static void BindProperty(SerializedObject so, string propName, List<InputActionReference> allRefs, string actionPath)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) return;

        var refProp = prop.FindPropertyRelative("m_Reference");
        if (refProp == null) refProp = prop.FindPropertyRelative("reference");

        if (refProp != null)
        {
            string actionName = actionPath.Split('/').Last();
            var actionRef = allRefs.FirstOrDefault(r => r.name == actionPath || r.name == actionName || r.name.EndsWith("/" + actionName));

            if (actionRef != null)
            {
                refProp.objectReferenceValue = actionRef;
            }
            else
            {
                Debug.LogWarning($"[FixVR] Could not find property reference for {actionPath}");
            }
        }
    }

    static void EnsureInputActionManager(InputActionAsset asset)
    {
        var manager = Object.FindAnyObjectByType<InputActionManager>();
        if (manager == null)
        {
            var go = new GameObject("XR Input Action Manager");
            manager = go.AddComponent<InputActionManager>();
            Debug.Log("[FixVR] Created XR Input Action Manager.");
        }

        var so = new SerializedObject(manager);
        var assetsProp = so.FindProperty("m_ActionAssets");
        
        bool found = false;
        for (int i = 0; i < assetsProp.arraySize; i++)
        {
            if (assetsProp.GetArrayElementAtIndex(i).objectReferenceValue == asset)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            assetsProp.arraySize++;
            assetsProp.GetArrayElementAtIndex(assetsProp.arraySize - 1).objectReferenceValue = asset;
            so.ApplyModifiedProperties();
            Debug.Log("[FixVR] XRI Default Input Actions added to Input Action Manager.");
        }
    }
}
