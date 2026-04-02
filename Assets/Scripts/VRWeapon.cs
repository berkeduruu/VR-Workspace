using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRWeapon : MonoBehaviour
{
    public enum FireMode { Semi, Auto }
    public FireMode fireMode = FireMode.Semi;
    public float fireRate = 0.1f;
    private float nextFireTime;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 40f;
    public float bulletLifeTime = 3f;

    public AudioClip fireSound;
    private AudioSource audioSource;

    public Magazine currentMagazine;
    private bool isTriggerDown = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null)
        {
            grab.activated.AddListener(OnTriggerDown);
            grab.deactivated.AddListener(OnTriggerUp);
        }
    }

    void Update()
    {
        if (isTriggerDown && fireMode == FireMode.Auto && Time.time >= nextFireTime)
        {
            ExecuteFire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void OnTriggerDown(ActivateEventArgs args)
    {
        isTriggerDown = true;
        if (fireMode == FireMode.Semi)
        {
            ExecuteFire();
        }
    }

    private void OnTriggerUp(DeactivateEventArgs args)
    {
        isTriggerDown = false;
    }

    private void ExecuteFire()
    {
        if (currentMagazine == null || !currentMagazine.HasAmmo()) return;
        if (firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = firePoint.forward * bulletSpeed;

        currentMagazine.UseAmmo();
        if (fireSound != null) audioSource.PlayOneShot(fireSound);

        Destroy(bullet, bulletLifeTime);
    }
}
