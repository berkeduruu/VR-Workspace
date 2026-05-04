using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixWarehouseMaterials : EditorWindow
{
    private const string TargetFolder = "Assets/UnityWarehouseSceneHDRP";
    private const string UrpLitShader = "Universal Render Pipeline/Lit";

    [MenuItem("Tools/Fix Warehouse Materials (URP)")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { TargetFolder });
        if (guids.Length == 0)
        {
            Debug.LogWarning("[FixWarehouseMaterials] No materials found in " + TargetFolder);
            return;
        }

        Shader urpLit = Shader.Find(UrpLitShader);
        if (urpLit == null)
        {
            Debug.LogError("[FixWarehouseMaterials] Could not find URP Lit shader.");
            return;
        }

        int converted = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            // Skip if already URP
            if (mat.shader != null && mat.shader.name.StartsWith("Universal Render Pipeline"))
                continue;

            // Cache properties
            Texture albedo = null;
            if (mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
            else if (mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");

            Color color = Color.white;
            if (mat.HasProperty("_Color")) color = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseColor")) color = mat.GetColor("_BaseColor");

            Texture normal = null;
            if (mat.HasProperty("_Normal_Map")) normal = mat.GetTexture("_Normal_Map");
            else if (mat.HasProperty("_BumpMap")) normal = mat.GetTexture("_BumpMap");

            float normalScale = 1.0f;
            if (mat.HasProperty("_Normal_Scale")) normalScale = mat.GetFloat("_Normal_Scale");
            else if (mat.HasProperty("_BumpScale")) normalScale = mat.GetFloat("_BumpScale");

            float smoothness = 0.5f;
            if (mat.HasProperty("_Smoothness_Max")) smoothness = mat.GetFloat("_Smoothness_Max");
            else if (mat.HasProperty("_Smoothness_Board")) smoothness = mat.GetFloat("_Smoothness_Board");
            else if (mat.HasProperty("_Glossiness")) smoothness = mat.GetFloat("_Glossiness");
            else if (mat.HasProperty("_Smoothness")) smoothness = mat.GetFloat("_Smoothness");

            Vector4 tiling = new Vector4(1, 1, 0, 0);
            if (mat.HasProperty("_Albedo_Tiling")) 
            {
                Vector4 t = mat.GetVector("_Albedo_Tiling");
                if (t.x != 0 && t.y != 0) tiling = new Vector4(t.x, t.y, 0, 0);
            }

            // Switch Shader
            mat.shader = urpLit;

            // Apply properties to URP Lit
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            
            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", normalScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            // Handle Tiling (URP uses _BaseMap_ST)
            if (albedo != null)
            {
                mat.SetTextureScale("_BaseMap", new Vector2(tiling.x, tiling.y));
            }

            EditorUtility.SetDirty(mat);
            converted++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FixWarehouseMaterials] Converted {converted} materials to URP Lit.");
        EditorUtility.DisplayDialog("Fix Warehouse Materials", $"Successfully converted {converted} materials!", "OK");
    }
}
