using UnityEngine;
using TMPro;

/// <summary>
/// AmmoHUD_ScreenCanvas üzerindeki TextMeshPro elemanını günceller.
/// VRWeaponKeyboard bu component'i bulur ve NotifyAmmoChanged() çağırır.
/// </summary>
public class AmmoHUDSimple : MonoBehaviour
{
    [Header("UI Referansı")]
    public TextMeshProUGUI ammoText;

    [Header("Renk Ayarları")]
    public Color normalColor  = Color.white;
    public Color lowAmmoColor = new Color(1f, 0.4f, 0.1f); // turuncu
    public Color emptyColor   = Color.red;
    public int   lowAmmoThreshold = 10;

    void Awake()
    {
        if (ammoText == null)
            ammoText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>VRWeaponKeyboard her ateş/reload sonrası bu metodu çağırır.</summary>
    public void NotifyAmmoChanged(int current, int max)
    {
        if (ammoText == null) return;

        ammoText.text = $"{current}  <size=70%>/ {max}</size>";

        if (current <= 0)
            ammoText.color = emptyColor;
        else if (current <= lowAmmoThreshold)
            ammoText.color = lowAmmoColor;
        else
            ammoText.color = normalColor;
    }

    /// <summary>Reload sırasında ekstra bilgi.</summary>
    public void ShowReloading()
    {
        if (ammoText == null) return;
        ammoText.text  = "↻ ...";
        ammoText.color = new Color(0.4f, 0.9f, 1f); // açık mavi
    }
}
