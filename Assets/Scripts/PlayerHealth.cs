using UnityEngine;
using EnemyAI;

public class PlayerHealth : HealthManager
{
    public float health = 500f;

    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart=null, GameObject origin=null)
    {
        health -= damage;
        Debug.Log("Player took damage! Current health: " + health);
        if (health <= 0)
        {
            dead = true;
            Debug.Log("Player is dead!");
        }
    }
}