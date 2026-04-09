using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Single AmmoWorldHUD system that supports per-weapon layout settings.
/// Assign a WeaponHUDSettings entry for each weapon prefab you want custom offsets for.
/// Falls back to "Default Settings" if no match is found.
/// </summary>
public class AmmoWorldHUD : MonoBehaviour
{
    // ─── per-weapon settings ──────────────────────────────────────────────────
    [Serializable]
    public class WeaponHUDSettings
    {
        [Tooltip("Drag the weapon GameObject or Prefab here to match")]
        public VRWeapon weaponReference;

        [Header("Sprites")]
        public Sprite spriteHasAmmo;
        public Sprite spriteNoAmmo;

        [Header("Offsets (weapon-local)")]
        [Tooltip("HUD position when held in the RIGHT hand")]
        public Vector3 rightHandOffset = new Vector3(-0.14f, 0.05f, 0f);
        [Tooltip("HUD position when held in the LEFT hand")]
        public Vector3 leftHandOffset  = new Vector3( 0.14f, 0.05f, 0f);

        [Header("Canvas")]
        public float   worldCanvasScale      = 0.0001f;
        public Vector2 worldAmmoTextPosition = new Vector2(20f, 95f);
    }

    [Header("Per-Weapon Settings")]
    [Tooltip("Add one entry per weapon type. Each weapon can have its own sprites and offsets.")]
    public List<WeaponHUDSettings> weaponSettings = new List<WeaponHUDSettings>();

    [Header("Default / Fallback Settings")]
    [Tooltip("Used when the held weapon has no matching entry in the list above")]
    public WeaponHUDSettings defaultSettings = new WeaponHUDSettings();

    // ─── global sprite fallbacks ──────────────────────────────────────────────
    [Header("Global Sprite Fallbacks")]
    public Sprite globalSpriteHasAmmo;
    public Sprite globalSpriteNoAmmo;

    // ─── hand transforms ──────────────────────────────────────────────────────
    [Header("Hand Transforms")]
    public Transform rightHand;
    public Transform leftHand;

    [Header("Screen HUD Settings")]
    public Vector2 screenAmmoTextPosition = new Vector2(20f, 35f);

    // ─── runtime references ───────────────────────────────────────────────────
    private Canvas          worldCanvas;
    private Image           worldBg;
    private TextMeshProUGUI worldAmmoTxt;

    private Canvas          screenCanvas;
    private Image           screenBg;
    private TextMeshProUGUI screenAmmoTxt;

    private VRWeapon trackedWeapon;
    private bool     weaponInRightHand;

    // ─── lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        CreateWorldCanvas();
        CreateScreenCanvas();
    }

    void Update()
    {
        DetectWeapon();

        bool hasWeapon = (trackedWeapon != null);
        ToggleGO(worldCanvas,  hasWeapon);
        ToggleGO(screenCanvas, !hasWeapon);

        if (hasWeapon) RefreshWorld();
        else           RefreshScreen();
    }

    // ─── detection ────────────────────────────────────────────────────────────
    void DetectWeapon()
    {
        trackedWeapon    = null;
        weaponInRightHand = false;

        VRWeapon[] all = FindObjectsByType<VRWeapon>(FindObjectsSortMode.None);
        foreach (VRWeapon w in all)
        {
            XRGrabInteractable grab = w.GetComponent<XRGrabInteractable>();
            if (grab == null || !grab.isSelected) continue;

            trackedWeapon = w;
            foreach (var itr in grab.interactorsSelecting)
            {
                Transform itrT = ((MonoBehaviour)itr).transform;
                if (rightHand != null && (itrT == rightHand || itrT.IsChildOf(rightHand)))
                {
                    weaponInRightHand = true;
                    break;
                }
            }
            break;
        }
    }

    // ─── resolve settings for the current weapon ──────────────────────────────
    WeaponHUDSettings GetSettings()
    {
        if (trackedWeapon == null) return defaultSettings;

        foreach (var s in weaponSettings)
        {
            // Match by instance or by prefab name (handles instantiated prefabs)
            if (s.weaponReference == null) continue;
            if (s.weaponReference == trackedWeapon) return s;

            // fallback: match by root name (prefab instance has "(Clone)" suffix)
            string cleanHeld = trackedWeapon.name.Replace("(Clone)", "").Trim();
            string cleanRef  = s.weaponReference.name.Replace("(Clone)", "").Trim();
            if (cleanHeld == cleanRef) return s;
        }

        return defaultSettings;
    }

    // ─── world HUD ────────────────────────────────────────────────────────────
    void RefreshWorld()
    {
        Magazine mag     = trackedWeapon.currentMagazine;
        bool     hasAmmo = mag != null && mag.HasAmmo();

        WeaponHUDSettings cfg = GetSettings();

        // Resolve sprites: per-weapon → global fallback
        Sprite sHas = cfg.spriteHasAmmo != null ? cfg.spriteHasAmmo : globalSpriteHasAmmo;
        Sprite sNo  = cfg.spriteNoAmmo  != null ? cfg.spriteNoAmmo  : globalSpriteNoAmmo;
        worldBg.sprite = hasAmmo ? sHas : sNo;

        // Ammo text
        worldAmmoTxt.text = mag != null ? (mag.ammoCount + "/" + mag.maxAmmoCount) : "0/0";

        // Canvas scale & text position from per-weapon config
        worldCanvas.transform.localScale           = Vector3.one * cfg.worldCanvasScale;
        worldAmmoTxt.rectTransform.anchoredPosition = cfg.worldAmmoTextPosition;

        // Position
        Vector3 localOffset = weaponInRightHand ? cfg.rightHandOffset : cfg.leftHandOffset;
        worldCanvas.transform.position = trackedWeapon.transform.TransformPoint(localOffset);

        // Billboard toward camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            worldCanvas.transform.LookAt(cam.transform);
            worldCanvas.transform.Rotate(0f, 180f, 0f);
        }
    }

    // ─── screen HUD ───────────────────────────────────────────────────────────
    void RefreshScreen()
    {
        if (screenBg      != null) screenBg.gameObject.SetActive(false);
        if (screenAmmoTxt != null) screenAmmoTxt.gameObject.SetActive(false);
    }

    // ─── builders ─────────────────────────────────────────────────────────────
    void CreateWorldCanvas()
    {
        var go = new GameObject("AmmoHUD_World");
        worldCanvas             = go.AddComponent<Canvas>();
        worldCanvas.renderMode  = RenderMode.WorldSpace;
        go.AddComponent<CanvasScaler>();
        go.transform.localScale = Vector3.one * 0.0001f;   // overridden each frame

        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(go.transform, false);
        worldBg              = bgGO.AddComponent<Image>();
        worldBg.preserveAspect = true;
        SetRect(bgGO, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(900, 600));

        var textGO = new GameObject("AmmoText");
        textGO.transform.SetParent(bgGO.transform, false);
        worldAmmoTxt           = textGO.AddComponent<TextMeshProUGUI>();
        worldAmmoTxt.fontSize  = 120;
        worldAmmoTxt.fontStyle = FontStyles.Bold;
        worldAmmoTxt.color     = Color.white;
        worldAmmoTxt.alignment = TextAlignmentOptions.Left;
        worldAmmoTxt.text      = "0/0";

        RectTransform rt  = textGO.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = defaultSettings.worldAmmoTextPosition;
        rt.sizeDelta        = new Vector2(380f, 120f);

        ToggleGO(worldCanvas, false);
    }

    void CreateScreenCanvas()
    {
        var go = new GameObject("AmmoHUD_Screen");
        screenCanvas              = go.AddComponent<Canvas>();
        screenCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        screenCanvas.sortingOrder = 10;

        CanvasScaler scaler         = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(go.transform, false);
        screenBg               = bgGO.AddComponent<Image>();
        screenBg.preserveAspect = true;

        RectTransform bgRect    = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(1f, 0f);
        bgRect.anchorMax        = new Vector2(1f, 0f);
        bgRect.pivot            = new Vector2(1f, 0f);
        bgRect.anchoredPosition = new Vector2(-30f, 30f);
        bgRect.sizeDelta        = new Vector2(320f, 213f);

        var textGO = new GameObject("AmmoText");
        textGO.transform.SetParent(bgGO.transform, false);
        screenAmmoTxt           = textGO.AddComponent<TextMeshProUGUI>();
        screenAmmoTxt.fontSize  = 38;
        screenAmmoTxt.fontStyle = FontStyles.Bold;
        screenAmmoTxt.color     = Color.white;
        screenAmmoTxt.alignment = TextAlignmentOptions.Left;
        screenAmmoTxt.text      = "---";

        RectTransform rt  = textGO.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = screenAmmoTextPosition;
        rt.sizeDelta        = new Vector2(160f, 50f);

        screenBg.gameObject.SetActive(false);
        screenAmmoTxt.gameObject.SetActive(false);
        ToggleGO(screenCanvas, false);
    }

    // ─── helpers ──────────────────────────────────────────────────────────────
    static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        RectTransform rt    = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
    }

    static void ToggleGO(Component c, bool val)
    {
        if (c != null && c.gameObject.activeSelf != val)
            c.gameObject.SetActive(val);
    }
}
