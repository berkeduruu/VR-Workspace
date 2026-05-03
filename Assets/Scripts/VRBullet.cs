using UnityEngine;
using EnemyAI;

public class VRBullet : MonoBehaviour
{
    public float damage = 20f;
    public GameObject impactEffect;

    private void OnCollisionEnter(Collision collision)
    {
        // Try to find HealthManager on the hit object or its root
        HealthManager health = collision.collider.GetComponentInParent<HealthManager>();
        
        if (health != null)
        {
            health.TakeDamage(collision.contacts[0].point, -collision.contacts[0].normal, damage, collision.collider, gameObject);
        }

        // Optional: spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthManager health = other.GetComponentInParent<HealthManager>();
        
        if (health != null)
        {
            health.TakeDamage(transform.position, transform.forward, damage, other, gameObject);
        }

        Destroy(gameObject);
    }
}
