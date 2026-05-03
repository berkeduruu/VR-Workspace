using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Tam otomatik silah scripti.
/// - VR: Silahı XRGrabInteractable ile tut, tetikle ateş et.
/// - Klavye/Fare: Sol Tık ile ateş et (XR Device Simulator'da da çalışır).
/// - Sonsuz mermi, şarjör yok.
/// - Raycast tabanlı, düşmanlara anında hasar verir.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VRWeaponSimple : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public float fireRate = 0.1f;          // Saniyede kaç kez ateş edebilir
    public float damage = 30f;              // Her merminin hasarı
    public float range = 200f;             // Menzil (metre)

    [Header("Referanslar")]
    public Transform firePoint;            // Namlu ucu (boş obje)
    public ParticleSystem muzzleFlash;     // Namlu alevi (opsiyonel)
    public GameObject impactEffect;        // Çarpma efekti (opsiyonel)

    [Header("Ses")]
    public AudioClip fireSound;

    // --- Private ---
    private float nextFireTime;
    private bool vrTriggerDown;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // VR Grab event'lerine bağlan (eğer XRGrabInteractable varsa)
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.activated.AddListener(OnVRTriggerDown);
            grab.deactivated.AddListener(OnVRTriggerUp);
        }
    }

    void Update()
    {
        bool wantsToFire = vrTriggerDown || Input.GetMouseButton(0);

        if (wantsToFire && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    // --- VR Trigger Callbacks ---
    private void OnVRTriggerDown(ActivateEventArgs args)
    {
        vrTriggerDown = true;
    }

    private void OnVRTriggerUp(DeactivateEventArgs args)
    {
        vrTriggerDown = false;
    }

    // --- Ateş Mekaniği ---
    void Fire()
    {
        if (firePoint == null) return;

        // Ses
        if (fireSound != null) audioSource.PlayOneShot(fireSound);

        // Namlu alevi
        if (muzzleFlash != null) muzzleFlash.Play();

        // Raycast
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            // Çarpma efekti
            if (impactEffect != null)
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            // Hasar
            HealthManager health = hit.collider.GetComponentInParent<HealthManager>();
            if (health != null)
                health.TakeDamage(hit.point, -hit.normal, damage, hit.collider, gameObject);
        }
    }
}
