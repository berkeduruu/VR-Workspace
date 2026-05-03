using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ekranın ortasında 3 saniyelik dairesel (radial fill) reload progress bar.
/// VRWeaponKeyboard tarafından StartReload() ile tetiklenir.
/// </summary>
public class ReloadUI : MonoBehaviour
{
    [Header("Referanslar")]
    public Image  radialFill;      // Image type = Filled, Radial 360

    [Header("Ayarlar")]
    public float reloadDuration = 3f;

    private Coroutine activeCoroutine;

    void Awake()
    {
        SetVisible(false);
    }

    /// <summary>VRWeaponKeyboard bu metodu çağırır.</summary>
    public void StartReload(float duration, System.Action onComplete)
    {
        reloadDuration = duration;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(DoReload(onComplete));
    }

    IEnumerator DoReload(System.Action onComplete)
    {
        SetVisible(true);
        float elapsed = 0f;
        while (elapsed < reloadDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / reloadDuration);
            if (radialFill != null) radialFill.fillAmount = t;
            yield return null;
        }
        SetVisible(false);
        onComplete?.Invoke();
    }

    void SetVisible(bool v)
    {
        if (radialFill != null) radialFill.gameObject.SetActive(v);
    }
}
