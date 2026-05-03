using UnityEngine;
using UnityEditor;
using EnemyAI;

/// <summary>
/// Sahneden eski silahları kaldırır ve Weapon_02'yi VR Player'ın sağ eline yerleştirir.
/// Tools → Setup Weapon_02 menüsünden çalıştır.
/// </summary>
public class SetupWeapon02 : MonoBehaviour
{
    [MenuItem("Tools/Setup Weapon_02 as VR Weapon")]
    public static void Setup()
    {
        // 1. Eski silahları ve şarjörleri sahneden sil
        string[] toDelete = new string[]
        {
            "M4_rifle", "Pistol", "pistol_magazine", "M4_magazine",
            "M4 rifle", "pistol magazine", "M4 magazine"
        };

        foreach (string name in toDelete)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
                Debug.Log($"Silindi: {name}");
            }
        }

        // 2. VR Player'ı ve Right Controller'ı bul
        GameObject vrPlayer = GameObject.Find("VR Player");
        if (vrPlayer == null)
        {
            Debug.LogError("VR Player sahnede bulunamadı! Önce 'Tools → Convert FPS Scene to VR' çalıştır.");
            return;
        }

        // Right Controller'ı bul (XR Origin yapısında farklı isimler olabilir)
        Transform rightController = FindDeepChild(vrPlayer.transform, "Right Controller");
        if (rightController == null) rightController = FindDeepChild(vrPlayer.transform, "RightHand Controller");
        if (rightController == null) rightController = FindDeepChild(vrPlayer.transform, "Right Hand");
        if (rightController == null)
        {
            Debug.LogWarning("Right Controller bulunamadı. Weapon_02 VR Player'ın köküne ekleniyor.");
            rightController = vrPlayer.transform;
        }

        // 3. Weapon_02 prefab'ını yükle
        string prefabPath = "Assets/LowPolyWeapons_LITE/Prefabs/Weapon_02.prefab";
        GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (weaponPrefab == null)
        {
            Debug.LogError($"Weapon_02 prefab'ı bulunamadı: {prefabPath}");
            return;
        }

        // 4. Silahı sahneye koy, Right Controller'a bağla
        GameObject weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
        Undo.RegisterCreatedObjectUndo(weapon, "Add Weapon_02");
        weapon.transform.SetParent(rightController, false);
        weapon.transform.localPosition = new Vector3(0f, -0.1f, 0.2f);
        weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        weapon.name = "Weapon_02";

        // 5. Mesh Collider'ları convex yap (dinamik Rigidbody ile uyumsuzluk için)
        foreach (MeshCollider mc in weapon.GetComponentsInChildren<MeshCollider>(true))
        {
            mc.convex = true;
        }

        // 6. Rigidbody ekle (yoksa)
        Rigidbody rb = weapon.GetComponent<Rigidbody>();
        if (rb == null) rb = weapon.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Kontrolcüye bağlı olduğu için kinematic

        // 7. BoxCollider ekle (grab için)
        BoxCollider bc = weapon.GetComponent<BoxCollider>();
        if (bc == null)
        {
            bc = weapon.AddComponent<BoxCollider>();
            bc.size = new Vector3(0.15f, 0.1f, 0.4f);
            bc.center = new Vector3(0f, 0f, 0.1f);
        }

        // 8. XRGrabInteractable ekle
        var grab = weapon.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab == null)
        {
            grab = weapon.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;

        // 9. FirePoint oluştur (silahın namlu ucunda)
        Transform existingFirePoint = weapon.transform.Find("FirePoint");
        if (existingFirePoint == null)
        {
            GameObject firePointGO = new GameObject("FirePoint");
            Undo.RegisterCreatedObjectUndo(firePointGO, "Add FirePoint");
            firePointGO.transform.SetParent(weapon.transform, false);
            firePointGO.transform.localPosition = new Vector3(0f, 0.02f, 0.5f); // Namlu ucu
            existingFirePoint = firePointGO.transform;
        }

        // 10. VRWeaponSimple ekle
        VRWeaponSimple weaponScript = weapon.GetComponent<VRWeaponSimple>();
        if (weaponScript == null) weaponScript = weapon.AddComponent<VRWeaponSimple>();
        weaponScript.firePoint = existingFirePoint;
        weaponScript.damage = 35f;
        weaponScript.fireRate = 0.1f;
        weaponScript.range = 200f;
        EditorUtility.SetDirty(weaponScript);

        // 11. Düşmanların aim target'ını VR Player'a yönelt (eğer değişmediyse)
        StateController[] enemies = Object.FindObjectsByType<StateController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.aimTarget == null)
            {
                enemy.aimTarget = vrPlayer.transform;
                EditorUtility.SetDirty(enemy);
            }
        }

        Debug.Log("✓ Weapon_02 kurulumu tamamlandı!");
        Debug.Log($"  → Eklendi: {rightController.name}/{weapon.name}");
        Debug.Log($"  → FirePoint: {existingFirePoint.localPosition}");
        Debug.Log("  → Play'e bas, Sol Tık veya VR Trigger ile ateş et!");

        Selection.activeGameObject = weapon;
    }

    // Recursive child search
    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
