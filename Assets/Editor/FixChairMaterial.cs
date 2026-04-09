using UnityEngine;
using UnityEditor;

public class FixChairMaterial : MonoBehaviour
{
    // Try again with different menu path
    [MenuItem("VR-Tools/Fix Chair Material")]
    public static void Fix()
    {
        // Find instance_31
        GameObject[] gos = Object.FindObjectsOfType<GameObject>();
        GameObject chair31 = null;
        foreach(var go in gos) {
            if(go.name == "instance_31") {
                chair31 = go;
                break;
            }
        }

        if (chair31 == null)
        {
            Debug.LogError("Could not find instance_31");
            return;
        }

        MeshRenderer[] renderers31 = chair31.GetComponentsInChildren<MeshRenderer>(true);
        if (renderers31.Length == 0)
        {
            Debug.LogError("No MeshRenderer found inside instance_31");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("Color_M07 t:Material");
        if (guids.Length == 0)
        {
            Debug.LogError("Could not find Color_M07.mat");
            return;
        }
        
        Material m07 = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));

        int replaced = 0;
        foreach (MeshRenderer mr in renderers31)
        {
            Material[] mats = mr.sharedMaterials; // need fresh copy
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].name.Contains("material_0"))
                {
                    mats[i] = m07;
                    changed = true;
                    replaced++;
                }
            }
            if(changed) {
                mr.sharedMaterials = mats;
                EditorUtility.SetDirty(mr);
            }
        }

        Debug.Log($"Replaced {replaced} materials on instance_31 to Color_M07");
    }
}
