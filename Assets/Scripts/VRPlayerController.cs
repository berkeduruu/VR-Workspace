using System.Collections;
using UnityEngine;

/// <summary>
/// XR Origin tabanli VR Player scripti.
/// VR modunda kamera ve eller TrackedPoseDriver tarafindan yonetilir,
/// bu script sadece saglik sistemi ve AI entegrasyonu icin kullanilir.
/// Editorde (XR Device Simulator yoksa) klavye/fare fallback saglar.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRPlayerController : HealthManager
{
    [Header("Saglik")]
    public float maxHealth = 200f;
    
    [Header("Referanslar")]
    public Transform cameraTransform;

    private float currentHealth;
    private bool isVRActive = false;

    void Awake()
    {
        currentHealth = maxHealth;
        
        // VR aktif mi kontrol et - eger XR subsystem yukluyse fare kontrolunu devre disi birak
        CheckVRStatus();
    }

    void CheckVRStatus()
    {
        // XR cihazi bagliysa (Quest, simulator, vs.) TrackedPoseDriver kamerayi yonetir
        // Bu durumda scriptin kameraya dokunmamasi gerekir
        var xrDisplaySubsystems = new System.Collections.Generic.List<UnityEngine.XR.XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(xrDisplaySubsystems);
        
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                isVRActive = true;
                Debug.Log("[VRPlayerController] XR cihazi algilandi - kamera kontrolu TrackedPoseDriver'a birakildi.");
                return;
            }
        }
        
        Debug.Log("[VRPlayerController] XR cihazi bulunamadi - TrackedPoseDriver varsa o yonetecek.");
    }

    void Update()
    {
        // VR modunda kameraya DOKUNMA - TrackedPoseDriver her seyi halleder
        // Hareket de XR Interaction Toolkit'in ContinuousMoveProvider'i tarafindan yonetilir
        // Bu script sadece saglik/hasar yonetimi icin
    }

    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart=null, GameObject origin=null)
    {
        if (dead) return;
        currentHealth -= damage;
        Debug.Log($"[VRPlayer] Hasar alindi: {damage} | Kalan saglik: {currentHealth}");
        if (currentHealth <= 0f)
        {
            dead = true;
            Debug.Log("[VRPlayer] Oyuncu oldu!");
        }
    }
}
