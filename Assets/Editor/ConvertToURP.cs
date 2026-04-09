using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Converts all Standard / Standard (Specular) materials under a chosen folder
/// to URP Lit shader, carrying over the main properties.
/// Run via:  Tools ▶ Convert Folder Materials to URP Lit
/// </summary>
public class ConvertToURP : EditorWindow
{
    private static readonly string k_urpLit   = "Universal Render Pipeline/Lit";
    private static readonly string k_urpSimple = "Universal Render Pipeline/Simple Lit";

    [MenuItem("Tools/Convert Office Materials to URP Lit")]
    public static void Run()
    {
        string folder = "Assets/Prefabs/Kurtlar+Vadisi+Pusu-Polat+Alemdar+Ofis/Materials";

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });

        if (guids.Length == 0)
        {
            Debug.LogWarning("[ConvertToURP] No materials found in " + folder);
            return;
        }

        Shader urpLit = Shader.Find(k_urpLit);
        if (urpLit == null)
        {
            Debug.LogError("[ConvertToURP] Could not find URP Lit shader. Is URP installed?");
            return;
        }

        int converted = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string shaderName = mat.shader != null ? mat.shader.name : "";

            // Skip if already URP
            if (shaderName.StartsWith("Universal Render Pipeline"))
            {
                skipped++;
                continue;
            }

            // Cache old values before switching shader
            Color   albedoColor   = mat.HasProperty("_Color")               ? mat.GetColor("_Color")               : Color.white;
            Texture albedoTex     = mat.HasProperty("_MainTex")             ? mat.GetTexture("_MainTex")            : null;
            Texture normalTex     = mat.HasProperty("_BumpMap")             ? mat.GetTexture("_BumpMap")            : null;
            float   normalScale   = mat.HasProperty("_BumpScale")           ? mat.GetFloat("_BumpScale")            : 1f;
            float   metallic      = mat.HasProperty("_Metallic")            ? mat.GetFloat("_Metallic")             : 0f;
            float   smoothness    = mat.HasProperty("_Glossiness")          ? mat.GetFloat("_Glossiness")           : 0.5f;
            Color   emissionColor = mat.HasProperty("_EmissionColor")       ? mat.GetColor("_EmissionColor")        : Color.black;
            Texture emissionTex   = mat.HasProperty("_EmissionMap")         ? mat.GetTexture("_EmissionMap")        : null;
            Texture metallicTex   = mat.HasProperty("_MetallicGlossMap")    ? mat.GetTexture("_MetallicGlossMap")   : null;
            Texture occlusionTex  = mat.HasProperty("_OcclusionMap")        ? mat.GetTexture("_OcclusionMap")       : null;
            float   occlusionStr  = mat.HasProperty("_OcclusionStrength")   ? mat.GetFloat("_OcclusionStrength")    : 1f;
            int     srcBlend      = mat.HasProperty("_SrcBlend")            ? (int)mat.GetFloat("_SrcBlend")        : 1;
            int     dstBlend      = mat.HasProperty("_DstBlend")            ? (int)mat.GetFloat("_DstBlend")        : 0;
            float   alpha         = mat.HasProperty("_Mode")                ? mat.GetFloat("_Mode")                 : 0f; // 0=Opaque,1=Cutout,2=Fade,3=Transparent

            // Switch to URP Lit
            mat.shader = urpLit;

            // Base color
            if (mat.HasProperty("_BaseColor"))    mat.SetColor("_BaseColor",   albedoColor);
            if (mat.HasProperty("_BaseMap"))      mat.SetTexture("_BaseMap",   albedoTex);

            // Normal
            if (normalTex != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetFloat("_BumpScale", normalScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            // Metallic / Smoothness
            if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic",    metallic);
            if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness",  smoothness);
            if (metallicTex != null && mat.HasProperty("_MetallicGlossMap"))
            {
                mat.SetTexture("_MetallicGlossMap", metallicTex);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            // Occlusion
            if (occlusionTex != null && mat.HasProperty("_OcclusionMap"))
            {
                mat.SetTexture("_OcclusionMap", occlusionTex);
                mat.SetFloat("_OcclusionStrength", occlusionStr);
            }

            // Emission
            if (emissionColor != Color.black || emissionTex != null)
            {
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
                if (emissionTex != null && mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", emissionTex);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            // Surface type (opaque/transparent)
            // URP uses _Surface: 0=Opaque, 1=Transparent
            bool transparent = (alpha >= 2f) || (srcBlend == 5 && dstBlend == 10); // Fade/Transparent mode
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", transparent ? 1f : 0f);
                if (transparent)
                {
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }

            EditorUtility.SetDirty(mat);
            converted++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ConvertToURP] Done! Converted: {converted}, Skipped (already URP): {skipped}. Total processed: {guids.Length}");
        EditorUtility.DisplayDialog("Convert to URP",
            $"Done.\nConverted: {converted}\nSkipped (already URP): {skipped}", "OK");
    }
}
