using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// For each material in the Office folder whose _BaseMap is null,
/// searches the Textures/ sub-folder for a texture matching the material name
/// and assigns it to _BaseMap + _BaseColor.
/// Run via: Tools ▶ Auto-Assign Office Textures
/// </summary>
public static class AutoAssignOfficeTextures
{
    [MenuItem("Tools/Auto-Assign Office Textures")]
    public static void Run()
    {
        string matFolder = "Assets/Prefabs/Kurtlar+Vadisi+Pusu-Polat+Alemdar+Ofis/Materials";
        string texFolder = "Assets/Prefabs/Kurtlar+Vadisi+Pusu-Polat+Alemdar+Ofis/Textures";

        // Build a name -> Texture2D lookup (lower-case key for case-insensitive matching)
        string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { texFolder });
        var texLookup = new Dictionary<string, Texture2D>();
        foreach (string tGuid in texGuids)
        {
            string tPath = AssetDatabase.GUIDToAssetPath(tGuid);
            string tName = Path.GetFileNameWithoutExtension(tPath).ToLowerInvariant();
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(tPath);
            if (tex != null && !texLookup.ContainsKey(tName))
                texLookup[tName] = tex;
        }

        Debug.Log($"[AutoAssign] Found {texLookup.Count} textures in Textures folder.");

        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });
        int assigned = 0, notFound = 0;

        foreach (string mGuid in matGuids)
        {
            string mPath = AssetDatabase.GUIDToAssetPath(mGuid);
            Material mat  = AssetDatabase.LoadAssetAtPath<Material>(mPath);
            if (mat == null) continue;

            // Only process URP Lit mats with no _BaseMap
            if (mat.shader == null || !mat.shader.name.StartsWith("Universal Render Pipeline"))
                continue;

            Texture existing = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
            if (existing != null) continue; // already assigned

            // Try to find texture by mat name (strip " 1" suffix copies)
            string matName = mat.name
                .Replace(" 1", "")   // duplicates created during import
                .ToLowerInvariant()
                .Trim();

            Texture2D found = null;

            // 1. Exact match
            if (texLookup.TryGetValue(matName, out found)) { }
            else
            {
                // 2. Partial match: find first texture whose name contains the mat name or vice versa
                foreach (var kvp in texLookup)
                {
                    if (kvp.Key.Contains(matName) || matName.Contains(kvp.Key))
                    {
                        found = kvp.Value;
                        break;
                    }
                }
            }

            if (found != null)
            {
                mat.SetTexture("_BaseMap", found);
                mat.SetColor("_BaseColor", Color.white); // let texture determine color
                EditorUtility.SetDirty(mat);
                assigned++;
            }
            else
            {
                notFound++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AutoAssign] Done. Assigned textures to {assigned} materials. " +
                  $"{notFound} materials had no matching texture (may use color-only).");
        EditorUtility.DisplayDialog("Auto-Assign Office Textures",
            $"Assigned textures to {assigned} materials.\n{notFound} used color only (no texture match).", "OK");
    }
}
