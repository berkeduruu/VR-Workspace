using UnityEngine;

/// <summary>
/// Mermi scripti:
/// - EnemyHealth.TakeDamage() cagirir (headshot dahil)
/// - TrailRenderer ile parlayan iz
/// - Hem Collision hem Trigger destegi
/// </summary>
public class VRBullet : MonoBehaviour
{
    public float damage = 35f;
    public GameObject impactEffect;

    void Awake()
    {
        // ---- Trail Renderer ----
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = gameObject.AddComponent<TrailRenderer>();

        trail.time             = 0.15f;
        trail.startWidth       = 0.04f;
        trail.endWidth         = 0.005f;
        trail.minVertexDistance = 0.05f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows   = false;

        // URP-compatible unlit material
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");

        Material mat = new Material(unlitShader);
        // Bright orange-yellow muzzle flash color
        mat.color = new Color(1f, 0.7f, 0.05f, 1f);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", new Color(1f, 0.7f, 0.05f, 1f));

        trail.material     = mat;
        trail.startColor   = new Color(1f, 0.85f, 0.1f, 1f);
        trail.endColor     = new Color(1f, 0.3f,  0f,   0f);
    }

    void OnCollisionEnter(Collision col)
    {
        DealDamage(col.collider, col.contacts[0].point, -col.contacts[0].normal);
        SpawnEffect(col.contacts[0].point, col.contacts[0].normal);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        DealDamage(other, transform.position, -transform.forward);
        Destroy(gameObject);
    }

    void DealDamage(Collider col, Vector3 point, Vector3 dir)
    {
        // Walk up parent chain
        HealthManager health = col.GetComponentInParent<HealthManager>();
        if (health != null)
        {
            health.TakeDamage(point, dir, damage, col, gameObject);
            Debug.Log($"[VRBullet] Hit {col.name} for {damage} dmg (HealthManager)");
            return;
        }

        // Try VRFPSKit interface
        VRFPSKit.IDamageReciever dr = col.GetComponentInParent<VRFPSKit.IDamageReciever>();
        if (dr != null)
        {
            dr.TakeDamage(damage);
            Debug.Log($"[VRBullet] Hit {col.name} for {damage} dmg (IDamageReciever)");
        }
    }

    void SpawnEffect(Vector3 point, Vector3 normal)
    {
        if (impactEffect != null)
            Instantiate(impactEffect, point, Quaternion.LookRotation(normal));
    }
}
