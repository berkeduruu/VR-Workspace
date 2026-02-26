using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
public class Basketball_Throw : MonoBehaviour
{
    [Header("Basketbol Ayarları")]
    [Tooltip("Potanın içindeki Trigger Collider'ın Tag değeri")]
    public string hoopTag = "Hoop";
    public bool requireDownwardMovement = true;

    [Header("Fırlatma / Çizgi Ayarları")]
    [Tooltip("Fırlatma kuvveti")]
    public float throwForce = 10f;
    [Tooltip("Çizginin görüneceği nokta sayısı (Kavis detayı)")]
    public int lineSegmentCount = 30;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private LineRenderer lineRenderer;
    private bool isGrabbed = false;

    // Topu tutan kontrolcü (el) referansı
    private Transform interactorTransform;

    // Fırlatma anında kullanılacak hız vektörü
    private Vector3 currentThrowVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // ÖNEMLİ: XR'ın kendi fırlatma hızını devre dışı bırakıyoruz,
        // çünkü biz kendi kavis hesabımızla fırlatacağız.
        if (grabInteractable != null)
        {
            grabInteractable.throwOnDetach = false;
        }

        // LineRenderer ayarı
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.enabled = false;
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0f, 0.8f, 1f); // Açık mavi
        lineRenderer.endColor = new Color(1f, 1f, 0f);      // Sarı
        lineRenderer.positionCount = lineSegmentCount;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false; // Başlangıçta KAPALI

        // XR Grab eventlerini bağla
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    // ========== GRAB / RELEASE ==========

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Eğer tutan şey bir Socket (yuva) ise işlem yapma
        if (args.interactorObject is XRSocketInteractor) return;

        lineRenderer.enabled = true;
        isGrabbed = true;
        interactorTransform = args.interactorObject.transform;
        Debug.Log("🤚 Top tutuldu - kavis çizgisi aktif.");
    }

    // Top bırakıldığında (G tuşunu bıraktığınızda) otomatik olarak kavis yönünde fırlatılır!
    private void OnRelease(SelectExitEventArgs args)
    {
        // Eğer bırakan şey bir Socket (yuva) ise fırlatma yapma
        if (args.interactorObject is XRSocketInteractor) return;

        isGrabbed = false;
        lineRenderer.enabled = false;

        // Topu bıraktığımız anda, son hesaplanan kavis yönüne doğru fırlat
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = currentThrowVelocity;

        Debug.Log("🏀 Top kavis çizgisine göre fırlatıldı! Güç: " + currentThrowVelocity.magnitude);
        interactorTransform = null;
    }

    // ========== UPDATE: Çizgiyi çiz ==========

    private void Update()
    {
        if (!isGrabbed || interactorTransform == null)
        {
            // Ek güvenlik: top tutulmuyorsa çizgiyi kapat
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;
            return;
        }

        // Kontrolcünün (elinizin) baktığı yönde, hafif yukarı açılı bir fırlatma vektörü hesapla
        Vector3 forward = interactorTransform.forward;
        currentThrowVelocity = (forward + Vector3.up * 0.5f).normalized * throwForce;

        // Kavis çizgisini çiz
        DrawTrajectoryLine(transform.position, currentThrowVelocity);
    }

    // ========== Kavis Çizimi ==========

    private void DrawTrajectoryLine(Vector3 startPos, Vector3 velocity)
    {
        float timeStep = 0.1f;

        for (int i = 0; i < lineSegmentCount; i++)
        {
            float t = i * timeStep;
            // Fizik formülü: P = P0 + V*t + 0.5*g*t^2
            Vector3 point = startPos + velocity * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }
    }

    // ========== Pota Algılama ==========

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(hoopTag))
        {
            if (requireDownwardMovement)
            {
                if (rb.linearVelocity.y < 0f)
                {
                    BasketOldu();
                }
            }
            else
            {
                BasketOldu();
            }
        }
    }

    private void BasketOldu()
    {
        Debug.Log("🏀 BASKET! Başarılı Atış!");
        // GameManager.Instance.AddScore(3);
    }
}