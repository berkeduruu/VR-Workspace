using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Weapon-side ammo HUD using custom sprites.
/// ammo_asset_0 (green)  = weapon held + has ammo  → world-space beside weapon
/// ammo_asset_1 (red)    = weapon held + no ammo   → world-space beside weapon
/// ammo_asset_2 (grey)   = no weapon held           → screen-space bottom-right
/// </summary>
public class AmmoWorldHUD : MonoBehaviour
{
    [Header("Sprites (assign in Inspector)")]
    public Sprite spriteHasAmmo;
    public Sprite spriteNoAmmo;
    public Sprite spriteNoWeapon;

    [Header("Hand Transforms")]
    public Transform rightHand;
    public Transform leftHand;

    [Header("World HUD Settings")]
    public float sidePanelOffset = 0.14f;
    public float verticalOffset  = 0.05f;
    public float worldCanvasScale = 0.0001f;
    public Vector2 worldAmmoTextPosition = new Vector2(20f, 95f);

    [Header("Screen HUD Settings")]
    public Vector2 screenAmmoTextPosition = new Vector2(20f, 35f);

    // ─── runtime references ───────────────────────────────────────────────────
    private Canvas    worldCanvas;
    private Image     worldBg;
    private TextMeshProUGUI worldAmmoTxt;

    private Canvas    screenCanvas;
    private Image     screenBg;
    private TextMeshProUGUI screenAmmoTxt;

    private VRWeapon  trackedWeapon;
    private bool      weaponInRightHand;

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

        if (hasWeapon)
            RefreshWorld();
        else
            RefreshScreen();
    }

    // ─── detection ────────────────────────────────────────────────────────────
    void DetectWeapon()
    {
        trackedWeapon = null;
        VRWeapon[] all = FindObjectsByType<VRWeapon>(FindObjectsSortMode.None);

        foreach (VRWeapon w in all)
        {
            XRGrabInteractable grab = w.GetComponent<XRGrabInteractable>();
            if (grab == null || !grab.isSelected) continue;

            trackedWeapon = w;
            weaponInRightHand = false;

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

    // ─── world HUD ────────────────────────────────────────────────────────────
    void RefreshWorld()
    {
        Magazine mag = trackedWeapon.currentMagazine;
        bool hasAmmo  = mag != null && mag.HasAmmo();

        worldBg.sprite = hasAmmo ? spriteHasAmmo : spriteNoAmmo;

        if (mag != null)
            worldAmmoTxt.text = mag.ammoCount + "/" + mag.maxAmmoCount;
        else
            worldAmmoTxt.text = "0  0";

        // Apply scale and text position dynamically for real-time editor tweaking
        worldCanvas.transform.localScale = Vector3.one * worldCanvasScale;
        worldAmmoTxt.rectTransform.anchoredPosition = worldAmmoTextPosition;

        // Position: opposite side of grabbing hand
        Transform wt = trackedWeapon.transform;
        float sign = weaponInRightHand ? -1f : 1f; // right hand → panel on left
        Vector3 pos = wt.position + wt.right * (sign * sidePanelOffset) + Vector3.up * verticalOffset;
        worldCanvas.transform.position = pos;

        // Bill-board toward camera
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
        screenBg.sprite = spriteNoWeapon;
        screenAmmoTxt.text = "---";

        // Apply text position dynamically for real-time editor tweaking
        screenAmmoTxt.rectTransform.anchoredPosition = screenAmmoTextPosition;
    }

    // ─── builders ─────────────────────────────────────────────────────────────
    void CreateWorldCanvas()
    {
        var go = new GameObject("AmmoHUD_World");
        worldCanvas = go.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        go.AddComponent<CanvasScaler>();

        // The reference layout is 900x600 units, scaled down via transform
        go.transform.localScale = Vector3.one * worldCanvasScale;

        // Background
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(go.transform, false);
        worldBg = bgGO.AddComponent<Image>();
        worldBg.preserveAspect = true;
        SetRect(bgGO, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(900, 600));

        // Ammo text — positioned in lower-center of card (matches "Mags:" label area)
        var textGO = new GameObject("AmmoText");
        textGO.transform.SetParent(bgGO.transform, false);
        worldAmmoTxt = textGO.AddComponent<TextMeshProUGUI>();
        worldAmmoTxt.fontSize  = 120;
        worldAmmoTxt.fontStyle = FontStyles.Bold;
        worldAmmoTxt.color     = Color.white;
        worldAmmoTxt.alignment = TextAlignmentOptions.Left;
        worldAmmoTxt.text      = "0  0";

        // Align to bottom-center of card: x ~300, y ~80 from bottom
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = worldAmmoTextPosition;
        rt.sizeDelta        = new Vector2(380f, 120f);

        ToggleGO(worldCanvas, false);
    }

    void CreateScreenCanvas()
    {
        var go = new GameObject("AmmoHUD_Screen");
        screenCanvas = go.AddComponent<Canvas>();
        screenCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        screenCanvas.sortingOrder = 10;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode      = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        // Background panel — bottom-right corner
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(go.transform, false);
        screenBg = bgGO.AddComponent<Image>();
        screenBg.preserveAspect = true;

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(1f, 0f);
        bgRect.anchorMax        = new Vector2(1f, 0f);
        bgRect.pivot            = new Vector2(1f, 0f);
        bgRect.anchoredPosition = new Vector2(-30f, 30f);
        bgRect.sizeDelta        = new Vector2(320f, 213f);

        // Text on top
        var textGO = new GameObject("AmmoText");
        textGO.transform.SetParent(bgGO.transform, false);
        screenAmmoTxt = textGO.AddComponent<TextMeshProUGUI>();
        screenAmmoTxt.fontSize  = 38;
        screenAmmoTxt.fontStyle = FontStyles.Bold;
        screenAmmoTxt.color     = Color.white;
        screenAmmoTxt.alignment = TextAlignmentOptions.Left;
        screenAmmoTxt.text      = "---";

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = screenAmmoTextPosition;
        rt.sizeDelta        = new Vector2(160f, 50f);

        ToggleGO(screenCanvas, false);
    }

    // ─── helpers ──────────────────────────────────────────────────────────────
    static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
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
