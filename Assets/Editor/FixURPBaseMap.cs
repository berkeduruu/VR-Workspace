using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes _BaseMap slot on URP Lit materials that have texture in _MainTex but not _BaseMap.
/// Run via: Tools ▶ Fix URP BaseMap Textures
/// </summary>
public static class FixURPBaseMap
{
    [MenuItem("Tools/Fix Office URP BaseMap Textures")]
    public static void Run()
    {
        string folder = "Assets/Prefabs/Kurtlar+Vadisi+Pusu-Polat+Alemdar+Ofis/Materials";
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });

        int fixed_count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            // Only touch URP Lit materials
            if (mat.shader == null || !mat.shader.name.StartsWith("Universal Render Pipeline"))
                continue;

            bool dirty = false;

            // If _BaseMap is null but _MainTex has something, copy it over
            Texture baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
            Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;

            if (baseMap == null && mainTex != null)
            {
                mat.SetTexture("_BaseMap", mainTex);
                dirty = true;
            }

            // If _BaseColor is white/default but _Color has a real color, copy it
            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            Color oldColor  = mat.HasProperty("_Color")     ? mat.GetColor("_Color")     : Color.white;

            if (baseColor == Color.white && oldColor != Color.white)
            {
                mat.SetColor("_BaseColor", oldColor);
                dirty = true;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(mat);
                fixed_count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FixURPBaseMap] Fixed {fixed_count} / {guids.Length} materials.");
        EditorUtility.DisplayDialog("Fix URP BaseMap", $"Fixed {fixed_count} materials!", "OK");
    }
}
