using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AmmoHUD : MonoBehaviour
{
    private TextMeshProUGUI ammoText;
    private XROrigin xrOrigin;
    private NearFarInteractor leftHandInteractor;
    private NearFarInteractor rightHandInteractor;

    private void Start()
    {
        xrOrigin = FindAnyObjectByType<XROrigin>();
        SetupUI();
    }

    private void SetupUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("AmmoHUD_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Text
        GameObject textGO = new GameObject("AmmoText");
        textGO.transform.SetParent(canvasGO.transform);
        
        ammoText = textGO.AddComponent<TextMeshProUGUI>();
        ammoText.fontSize = 48;
        ammoText.alignment = TextAlignmentOptions.BottomRight;
        ammoText.color = Color.white;
        ammoText.text = "---";

        // Position it in bottom right
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-50, 50); // 50px offset from right/bottom
        rect.sizeDelta = new Vector2(400, 100);
    }

    private void Update()
    {
        VRWeapon activeWeapon = FindHeldWeapon();

        if (activeWeapon != null)
        {
            if (activeWeapon.currentMagazine != null)
            {
                ammoText.text = $"{activeWeapon.currentMagazine.ammoCount}/{activeWeapon.currentMagazine.maxAmmoCount}";
            }
            else
            {
                ammoText.text = "0/0";
            }
        }
        else
        {
            ammoText.text = "---";
        }
    }

    private VRWeapon FindHeldWeapon()
    {
        // Try to find the weapon in any object that has a VRWeapon script and is being held.
        // We look for VRWeapon components on objects that are grabbed.
        VRWeapon[] allWeapons = FindObjectsByType<VRWeapon>(FindObjectsSortMode.None);
        
        foreach (var weapon in allWeapons)
        {
            var interactable = weapon.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (interactable != null && interactable.isSelected)
            {
                return weapon;
            }
        }
        
        return null;
    }
}
