using UnityEditor;
using UnityEngine;

public class RemoveLaserFromM16Auto
{
    [InitializeOnLoadMethod]
    public static void Remove()
    {
        string path = "Assets/VRFPS Kit/Prefabs/Items/Firearms/M16.prefab";
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            GameObject prefabRoot = editingScope.prefabContentsRoot;
            Transform bulletSpawn = prefabRoot.transform.Find("model/bullet spawn");
            if (bulletSpawn != null)
            {
                Transform laserRay = bulletSpawn.Find("Laser Ray");
                if (laserRay != null)
                {
                    Object.DestroyImmediate(laserRay.gameObject);
                    Debug.Log("Successfully removed Laser Ray from M16");
                }
            }
        }
    }
}
