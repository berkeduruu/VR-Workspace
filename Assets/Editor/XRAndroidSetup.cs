using UnityEditor;
using UnityEngine;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using System.Linq;

public static class XRAndroidSetup
{
    [MenuItem("Tools/Fix XR Settings For Android")]
    public static void SetupXRForAndroid()
    {
        // Get XR Settings for Android using the management editor assembly
        XRGeneralSettings generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        
        if (generalSettings == null)
        {
            // If settings don't exist, we try to get the overall config object
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            
            if (buildTargetSettings == null)
            {
                buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
            }

            generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            generalSettings.Manager = manager;

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/XR")) AssetDatabase.CreateFolder("Assets", "XR");
            if (!AssetDatabase.IsValidFolder("Assets/XR/Settings")) AssetDatabase.CreateFolder("Assets/XR", "Settings");

            AssetDatabase.CreateAsset(manager, "Assets/XR/Settings/AndroidXRManagerSettings.asset");
            AssetDatabase.CreateAsset(generalSettings, "Assets/XR/Settings/AndroidXRGeneralSettings.asset");
            
            buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Android, generalSettings);
            EditorUtility.SetDirty(buildTargetSettings);
        }

        if (generalSettings != null)
        {
            generalSettings.InitManagerOnStart = true;
            EditorUtility.SetDirty(generalSettings);

            var manager = generalSettings.Manager;
            if (manager != null)
            {
                // Find all loaders (Oculus, OpenXR, etc)
                string[] loaderGuids = AssetDatabase.FindAssets("t:XRLoader");
                foreach (string guid in loaderGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // Filter for common loaders
                    if (path.ToLower().Contains("oculus") || path.ToLower().Contains("openxr"))
                    {
                        XRLoader loader = AssetDatabase.LoadAssetAtPath<XRLoader>(path);
                        if (loader != null && !manager.activeLoaders.Contains(loader))
                        {
                            // Some loaders might be platform specific in their asset definitions
                            manager.TryAddLoader(loader);
                            Debug.Log("Added XR Loader to Android: " + loader.name);
                        }
                    }
                }
                EditorUtility.SetDirty(manager);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("XR Android Settings Fixed. You can now build for Quest!");
    }
}
