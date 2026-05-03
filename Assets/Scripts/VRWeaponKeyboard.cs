using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class VRWeaponKeyboard : MonoBehaviour
{
    [Header("Ates Ayarlari")]
    public float fireRate = 0.12f;
    public int maxAmmo = 30;
    public float reloadTime = 3f;

    [Header("Mermi & Efekt")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 60f;
    public float bulletDamage = 35f;

    [Header("Sesler")]
    public AudioClip fireSound;
    public AudioClip reloadSound;

    [Header("UI Referanslari")]
    public ReloadUI reloadUI;
    public AmmoHUDSimple ammoHUD;

    [Header("VR Takip (Kontrolcüler)")]
    public Transform rightHandTransform;
    public Transform leftHandTransform;

    [Header("Silah Soketleri (Silahın Üstündeki Noktalar)")]
    public Transform weaponRightSocket;
    public Transform weaponLeftSocket;

    [Header("Giris (Input)")]
    public InputActionReference fireAction;
    public InputActionReference reloadAction;

    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        if (fireAction != null && fireAction.action != null) fireAction.action.Enable();
        if (reloadAction != null && reloadAction.action != null) reloadAction.action.Enable();
    }

    void Start()
    {
        if (reloadUI == null) reloadUI = FindAnyObjectByType<ReloadUI>();
        if (ammoHUD == null) ammoHUD = FindAnyObjectByType<AmmoHUDSimple>();
        NotifyHUD();

        // Baslangicta eksik var mi kontrol et (Console'a bak)
        if (rightHandTransform == null || leftHandTransform == null) Debug.LogError("DİKKAT: Sağ veya Sol El Transformu atanmamış!");
        if (weaponRightSocket == null || weaponLeftSocket == null) Debug.LogError("DİKKAT: Silah üzerindeki sağ/sol soketler atanmamış!");
    }

    void Update()
    {
        if (isReloading) return;

        bool triggerPressed = false;
        
        // Input System Action kontrolü
        if (fireAction != null && fireAction.action != null)
            triggerPressed = fireAction.action.IsPressed() || fireAction.action.ReadValue<float>() > 0.5f;
        
        // Klavye desteği (Input System uyumlu)
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) triggerPressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) triggerPressed = true;

        if (currentAmmo <= 0 && triggerPressed) { BeginReload(); return; }
        if (triggerPressed && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }

        bool reloadPressed = false;
        if (reloadAction != null && reloadAction.action != null && reloadAction.action.WasPressedThisFrame()) reloadPressed = true;
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) reloadPressed = true;

        if (reloadPressed) BeginReload();
    }

    void LateUpdate()
    {
        if (rightHandTransform == null || leftHandTransform == null) return;

        // 1. Silahın merkezini direkt Sağ Ele (Tetik) kitle.
        transform.position = rightHandTransform.position;

        // 2. Silahın namlusunu Sol Ele doğru çevir. 
        Vector3 aimDirection = leftHandTransform.position - rightHandTransform.position;
        if (aimDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(aimDirection, rightHandTransform.up);
        }

        // 3. Ofset Uygulaması
        if (weaponRightSocket != null)
        {
            // Silahı, sağ soket elin olduğu yere gelecek şekilde kaydır
            Vector3 offset = transform.position - weaponRightSocket.position;
            transform.position += offset;
        }
    }

    void Fire()
    {
        currentAmmo--;
        if (fireSound != null) audioSource.PlayOneShot(fireSound);
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            var rb = b.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = firePoint.forward * bulletSpeed;
            Destroy(b, 5f);
        }
        NotifyHUD();
    }

    void BeginReload()
    {
        if (isReloading) return;
        isReloading = true;
        if (reloadSound != null) audioSource.PlayOneShot(reloadSound);
        if (reloadUI != null) reloadUI.StartReload(reloadTime, FinishReload);
        else StartCoroutine(DelayedFinishReload());
    }

    IEnumerator DelayedFinishReload() { yield return new WaitForSeconds(reloadTime); FinishReload(); }
    void FinishReload() { currentAmmo = maxAmmo; isReloading = false; NotifyHUD(); }
    void NotifyHUD() { if (ammoHUD != null) ammoHUD.NotifyAmmoChanged(currentAmmo, maxAmmo); }
}
