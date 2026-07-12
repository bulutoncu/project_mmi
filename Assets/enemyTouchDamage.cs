using UnityEngine;

// Damages the player on contact (at most once per cooldown).
public class EnemyTouchDamage : MonoBehaviour
{
    public float damage = 20f;
    public float cooldown = 1f;     // minimum seconds between hits
    float lastHit = -99f;

    void OnCollisionStay(Collision collision)
    {
        TryHit(collision.collider);
    }

    void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    void TryHit(Collider c)
    {
        if (Time.time - lastHit < cooldown) return;
        if (!c.CompareTag("Player")) return;

        var health = c.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            lastHit = Time.time;
            Debug.Log("💥 The enemy hit you! Health left: " + health.currentHealth);
        }
    }
}
