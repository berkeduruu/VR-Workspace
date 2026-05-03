using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using TMPro;

public class SetupVRPlayer : MonoBehaviour
{
    [MenuItem("Tools/Setup VR Player")]
    public static void Setup()
    {
        // 1. Deactivate old player
        GameObject oldPlayer = GameObject.Find("player");
        if (oldPlayer != null)
        {
            oldPlayer.SetActive(false);
            Debug.Log("Old player deactivated.");
        }

        // 2. Create VRPlayer Root
        GameObject vrPlayer = new GameObject("VRPlayer");
        vrPlayer.tag = "Player";
        vrPlayer.layer = LayerMask.NameToLayer("Default");

        CharacterController cc = vrPlayer.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0, 0.9f, 0);
        cc.radius = 0.3f;

        vrPlayer.AddComponent<AudioSource>();
        VRPlayerController playerCtrl = vrPlayer.AddComponent<VRPlayerController>();

        // 3. Create Camera Offset
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(vrPlayer.transform, false);
        cameraOffset.transform.localPosition = new Vector3(0, 1.6f, 0);

        // 4. Create Main Camera
        GameObject mainCamera = new GameObject("Main Camera");
        mainCamera.tag = "MainCamera";
        mainCamera.transform.SetParent(cameraOffset.transform, false);
        Camera cam = mainCamera.AddComponent<Camera>();
        mainCamera.AddComponent<AudioListener>();

        // Link camera to player controller
        playerCtrl.cameraTransform = mainCamera.transform;

        // Add XROrigin to VRPlayer
        XROrigin xrOrigin = vrPlayer.AddComponent<XROrigin>();
        xrOrigin.CameraFloorOffsetObject = cameraOffset;
        xrOrigin.Camera = cam;

        // 5. Create Hands
        GameObject leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(cameraOffset.transform, false);
        leftHand.transform.localPosition = new Vector3(-0.25f, -0.3f, 0.4f);

        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(cameraOffset.transform, false);
        rightHand.transform.localPosition = new Vector3(0.25f, -0.3f, 0.4f);

        // Load Hand Models
        GameObject leftHandModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Animated Hands/Prefabs/Left Hand Model.prefab");
        if (leftHandModelPrefab != null) {
            GameObject lhModel = (GameObject)PrefabUtility.InstantiatePrefab(leftHandModelPrefab);
            lhModel.transform.SetParent(leftHand.transform, false);
        }

        GameObject rightHandModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Animated Hands/Prefabs/Right Hand Model.prefab");
        if (rightHandModelPrefab != null) {
            GameObject rhModel = (GameObject)PrefabUtility.InstantiatePrefab(rightHandModelPrefab);
            rhModel.transform.SetParent(rightHand.transform, false);
        }

        // 6. Setup Weapon
        GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LowPolyWeapons_LITE/Prefabs/Weapon_02.prefab");
        GameObject weaponObj = null;
        if (weaponPrefab != null) {
            weaponObj = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
            weaponObj.transform.SetParent(rightHand.transform, false);
            weaponObj.transform.localPosition = new Vector3(0, 0, 0.1f);
            weaponObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

            var oldSimple = weaponObj.GetComponent<VRWeaponSimple>();
            if (oldSimple != null) DestroyImmediate(oldSimple);

            VRWeaponKeyboard weaponScript = weaponObj.AddComponent<VRWeaponKeyboard>();

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(weaponObj.transform, false);
            firePoint.transform.localPosition = new Vector3(0.6f, 0.1f, 0);
            firePoint.transform.localRotation = Quaternion.Euler(0, 90, 0);

            weaponScript.firePoint = firePoint.transform;

            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LowPolyWeapons_LITE/Prefabs/BulletLite_02.prefab");
            weaponScript.bulletPrefab = bulletPrefab;

            if (bulletPrefab != null) {
                var vrBullet = bulletPrefab.GetComponent<VRBullet>();
                if (vrBullet == null) {
                    bulletPrefab.AddComponent<VRBullet>();
                    EditorUtility.SetDirty(bulletPrefab);
                }
            }

            AudioClip fireSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Prefabs/m4a1_unsil-2.wav");
            weaponScript.fireSound = fireSound;
        }

        // 7. Setup UI Canvas for Reload
        GameObject reloadCanvasGO = new GameObject("ReloadUI_Canvas");
        Canvas reloadCanvas = reloadCanvasGO.AddComponent<Canvas>();
        reloadCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        reloadCanvasGO.AddComponent<CanvasScaler>();
        reloadCanvasGO.AddComponent<GraphicRaycaster>();

        GameObject reloadRingGO = new GameObject("ReloadRing");
        reloadRingGO.transform.SetParent(reloadCanvasGO.transform, false);
        Image reloadRing = reloadRingGO.AddComponent<Image>();
        reloadRing.type = Image.Type.Filled;
        reloadRing.fillMethod = Image.FillMethod.Radial360;
        reloadRing.fillAmount = 0f;
        reloadRing.color = new Color(1, 1, 1, 0.8f);
        
        RectTransform ringRect = reloadRingGO.GetComponent<RectTransform>();
        ringRect.anchorMin = new Vector2(0.5f, 0.5f);
        ringRect.anchorMax = new Vector2(0.5f, 0.5f);
        ringRect.sizeDelta = new Vector2(100, 100);

        /*
        GameObject reloadTextGO = new GameObject("ReloadText");
        reloadTextGO.transform.SetParent(reloadCanvasGO.transform, false);
        TextMeshProUGUI reloadText = reloadTextGO.AddComponent<TextMeshProUGUI>();
        reloadText.text = "Sarjor...";
        reloadText.alignment = TextAlignmentOptions.Center;
        reloadText.fontSize = 24;
        RectTransform textRect = reloadTextGO.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, -70);
        */

        ReloadUI reloadUIScript = reloadCanvasGO.AddComponent<ReloadUI>();
        reloadUIScript.radialFill = reloadRing;
        // reloadUIScript.reloadLabel = reloadText;

        // 8. Setup Ammo HUD
        GameObject ammoHUDPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/AmmoHUD_ScreenCanvas.prefab");
        if (ammoHUDPrefab != null) {
            GameObject ammoHUDObj = (GameObject)PrefabUtility.InstantiatePrefab(ammoHUDPrefab);
            
            var existingScript = ammoHUDObj.GetComponent<AmmoHUD>();
            if (existingScript != null) DestroyImmediate(existingScript);

            AmmoHUDSimple simpleHUD = ammoHUDObj.AddComponent<AmmoHUDSimple>();
            
            if (weaponObj != null) {
                var weaponScript = weaponObj.GetComponent<VRWeaponKeyboard>();
                weaponScript.reloadUI = reloadUIScript;
                weaponScript.ammoHUD = simpleHUD;
            }
        }

        Debug.Log("VR Player setup completed successfully!");
    }
}
