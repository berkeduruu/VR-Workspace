using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using EnemyAI;

/// <summary>
/// fight.unity sahnesini VR+Klavye destekli çatışma sahnesine çevirir.
/// Main sahnesinden sadece: VR Player, XR Interaction Manager, XR Device Simulator kopyalanır.
/// Silahlar, HUD vb. kopyalanmaz — silah VR Player'a programatik olarak eklenir.
/// </summary>
public class ConvertToVR
{
    [MenuItem("Tools/Convert FPS Scene to VR")]
    public static void ConvertScene()
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();

        // 1. Eski FPS karakterini temizle
        string[] toDelete = new string[]
        {
            "FirstPersonCharacter", "Player", "FPS Player",
            "VR Player", "XR Interaction Manager", "XR Device Simulator",
            "Locomotion System", "EventSystem (VR)", "AmmoHUD_System",
            "Pistol", "M4_rifle", "pistol_magazine", "M4_magazine"
        };

        foreach (string n in toDelete)
        {
            GameObject go = GameObject.Find(n);
            if (go != null) Undo.DestroyObjectImmediate(go);
        }

        // 2. Main sahnesinden VR Player + XR Manager + XR Simulator kopyala
        string[] guids = AssetDatabase.FindAssets("main t:Scene");
        if (guids.Length == 0) { Debug.LogError("main.unity bulunamadı!"); return; }

        string mainPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Scene mainScene = EditorSceneManager.OpenScene(mainPath, OpenSceneMode.Additive);

        string[] copyTargets = new string[]
        {
            "VR Player", "XR Interaction Manager", "XR Device Simulator"
        };

        GameObject vrPlayerRef = null;

        foreach (GameObject root in mainScene.GetRootGameObjects())
        {
            foreach (string target in copyTargets)
            {
                if (root.name == target)
                {
                    SceneManager.MoveGameObjectToScene(root, currentScene);
                    if (root.name == "VR Player")
                    {
                        vrPlayerRef = root;
                        root.transform.position = new Vector3(0, 0, 0);
                    }
                    break;
                }
            }
        }

        EditorSceneManager.CloseScene(mainScene, true);

        if (vrPlayerRef == null)
        {
            Debug.LogError("VR Player main.unity içinde bulunamadı!");
            return;
        }

        // 3. VR Player'a PlayerHealth ekle
        if (vrPlayerRef.GetComponent<PlayerHealth>() == null)
            vrPlayerRef.AddComponent<PlayerHealth>();

        // 4. Düşmanları VR Player'ı hedef alacak şekilde ayarla
        StateController[] enemies = Object.FindObjectsByType<StateController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.aimTarget = vrPlayerRef.transform;
            EditorUtility.SetDirty(enemy);
        }

        // 5. VR Kamerayı MainCamera olarak etiketle
        Camera vrCam = vrPlayerRef.GetComponentInChildren<Camera>();
        if (vrCam != null)
        {
            vrCam.tag = "MainCamera";
            // Çakışan diğer kameraları devre dışı bırak
            Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in allCams)
            {
                if (cam != vrCam) cam.gameObject.SetActive(false);
            }
        }

        // 6. Sahneyi kaydet
        EditorSceneManager.SaveScene(currentScene);

        Debug.Log("=== Dönüşüm tamamlandı! ===");
        Debug.Log("Şimdi VR Player'ın sağ eline gidip bir silah prefab'ı sürükleyin.");
        Debug.Log("Silaha VRWeaponSimple + XRGrabInteractable ekleyin.");
    }
}
