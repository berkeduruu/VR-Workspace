using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Sonsuz şarjör çantası (Ammo Pouch) mekaniği.
/// Belirli bir noktada (örn. oyuncunun belinde) sürekli yeni bir şarjör üretir.
/// Kullanıcı şarjörü eline aldığında kısa bir süre sonra yerine yenisi spawn olur.
/// </summary>
public class AmmoPouch : MonoBehaviour
{
    [Header("Pouch Ayarları")]
    [Tooltip("Çoğaltılacak şarjör prefab'ı (Pistol veya M4)")]
    public GameObject magazinePrefab;
    
    [Tooltip("Şarjörün belireceği nokta (boş bırakılırsa bu objenin kendi pozisyonu kullanılır)")]
    public Transform spawnPoint;
    
    [Tooltip("Şarjör alındıktan ne kadar süre sonra yenisi gelsin?")]
    public float respawnDelay = 0.5f;

    private GameObject currentMagazine;

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        SpawnMagazine();
    }

    void SpawnMagazine()
    {
        if (magazinePrefab == null)
        {
            Debug.LogWarning("AmmoPouch: Şarjör prefab'ı atanmadı!");
            return;
        }

        // Yeni şarjörü spawn et
        currentMagazine = Instantiate(magazinePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Pouch objesine bağla ki oyuncuyla beraber hareket etsin (beline takılıysa)
        currentMagazine.transform.SetParent(spawnPoint);

        // Beklerken yere düşmemesi için yerçekimini kapat / kinematic yap
        Rigidbody rb = currentMagazine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Oyuncunun tutup tutmadığını anlamak için XRGrabInteractable eventine bağlan
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = currentMagazine.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // Kullanıcı şarjörü eline aldığında OnMagazineGrabbed fonksiyonu çalışır
            grabInteractable.selectEntered.AddListener(OnMagazineGrabbed);
        }
    }

    void OnMagazineGrabbed(SelectEnterEventArgs args)
    {
        if (currentMagazine == null) return;

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = currentMagazine.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // Event listener'ı temizle ki tekrar tetiklenmesin
            grabInteractable.selectEntered.RemoveListener(OnMagazineGrabbed);
        }

        // Oyuncu şarjörü aldığı için pouch ile olan ebeveyn bağını kopar
        currentMagazine.transform.SetParent(null);

        // Fiziğini tekrar aç ki oyuncu elinden bırakırsa yere düşebilsin
        Rigidbody rb = currentMagazine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Referansı temizle
        currentMagazine = null;

        // Belirlenen süre (respawnDelay) geçtikten sonra yeni bir şarjör spawn et
        Invoke(nameof(SpawnMagazine), respawnDelay);
    }
}
