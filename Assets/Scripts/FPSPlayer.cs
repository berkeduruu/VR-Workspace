using UnityEngine;
using EnemyAI;

/// <summary>
/// Klavye+Fare ile kontrol edilebilen FPS oyuncu scripti.
/// WASD hareket, Fare bakış, Sol Tık ateş.
/// Bu script bir FPSPlayer objesine, kamera da o objenin içindeki kamera objesine bağlanır.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSPlayer : HealthManager
{
    [Header("Hareket")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Kamera / Bakış")]
    public Transform cameraTransform;     // Sahne içindeki kamerayı buraya sürükle
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Silah")]
    public float damage = 30f;           // Her ateşte verilen hasar
    public float fireRate = 0.15f;
    public float range = 200f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    [Header("Sağlık")]
    public float maxHealth = 200f;

    // --- Private ---
    private CharacterController cc;
    private Vector3 velocity;
    private float verticalRotation;
    private float nextFireTime;
    private int currentAmmo;
    private bool isReloading;
    private AudioSource audioSource;
    private float currentHealth;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        currentAmmo = maxAmmo;
        currentHealth = maxHealth;

        // Fareyi kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isReloading) return;

        HandleMovement();
        HandleMouseLook();
        HandleShooting();

        // ESC ile imleci geri aç (debug için)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Sol tık ile tekrar kilitle
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleMovement()
    {
        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0) velocity.y = -2f;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        cc.Move(move * speed * Time.deltaTime);

        // Zıplama
        if (Input.GetButtonDown("Jump") && grounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleShooting()
    {
        // R ile şarjör değiştir
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        if (!Input.GetMouseButton(0)) return;
        if (Time.time < nextFireTime) return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Fire();
        nextFireTime = Time.time + fireRate;
    }

    void Fire()
    {
        currentAmmo--;

        // Ses & efekt
        if (fireSound != null) audioSource.PlayOneShot(fireSound);
        if (muzzleFlash != null) muzzleFlash.Play();

        // Raycast ile hasar ver
        if (cameraTransform == null) return;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Impact efekti
            if (impactEffect != null)
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            // Hasar ver
            HealthManager health = hit.collider.GetComponentInParent<HealthManager>();
            if (health != null)
                health.TakeDamage(hit.point, -hit.normal, damage, hit.collider, gameObject);
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Şarjör değiştiriliyor...");
        if (reloadSound != null) audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Hazır! Mermi: " + currentAmmo);
    }

    // EnemyAI'nın HealthManager'ından gelen hasar
    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {
        currentHealth -= damage;
        Debug.Log($"Oyuncu hasar aldı! Can: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0f && !dead)
        {
            dead = true;
            Debug.Log("OYUNCU ÖLDÜ!");
            // İsteğe bağlı: GameOver ekranı
        }
    }

    void OnGUI()
    {
        // Basit HUD (debug amaçlı)
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 200, 25), $"Can: {Mathf.CeilToInt(currentHealth)} / {maxHealth}");
        GUI.Label(new Rect(10, 35, 200, 25), $"Mermi: {currentAmmo} / {maxAmmo}");
        if (isReloading)
            GUI.Label(new Rect(10, 60, 200, 25), "Şarjör değiştiriliyor...");

        // Nişangah
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        GUI.DrawTexture(new Rect(center.x - 1, center.y - 10, 2, 20), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(center.x - 10, center.y - 1, 20, 2), Texture2D.whiteTexture);
    }
}
