using UnityEngine;

public class LavaFloor : MonoBehaviour
{
    public float damagePerSecond = 2f;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}