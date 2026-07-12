using UnityEngine;

// Sets the player on fire while they stand on lava.
// The burning lasts a while after leaving the lava (handled by PlayerHealth)
// and can be put out by blowing into the mic or typing "blow".
public class LavaIgnite : MonoBehaviour
{
    public float burnDuration = 8f;   // lava fire burns longer than normal fire

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.CatchFire(burnDuration);
        }
    }
}
