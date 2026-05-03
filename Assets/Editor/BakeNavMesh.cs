using UnityEngine;
using UnityEditor;

public class BakeNavMesh
{
    [MenuItem("Tools/Bake NavMesh")]
    public static void Bake()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null) {
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);
        }
        
        GameObject[] covers = { GameObject.Find("Cover_1"), GameObject.Find("Cover_2"), GameObject.Find("Cover_3") };
        foreach(var c in covers) {
            if (c != null) GameObjectUtility.SetStaticEditorFlags(c, StaticEditorFlags.NavigationStatic);
        }

        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("NavMesh baked successfully!");
    }
}