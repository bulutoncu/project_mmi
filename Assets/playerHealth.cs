using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public bool isOnFire = false;
    public float fireInterval = 4f;       // how often you catch fire on your own
    public float fireDuration = 3f;       // how long the fire lasts
    public float fireDamageMoving = 10f;  // damage/second while moving
    public float fireDamageStill = 15f;   // damage/second while standing still

    private float fireTimer = 0f;
    private float fireDurationTimer = 0f;
    private float activeFireDuration = 3f;   // duration of the current fire (lava burns longer)

    void Start()
    {
        currentHealth = maxHealth;
        activeFireDuration = fireDuration;
    }

    void Update()
    {
        HandleFireTiming();

        if (isOnFire)
        {
            bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            float damage = isMoving ? fireDamageMoving : fireDamageStill;
            TakeDamage(damage * Time.deltaTime);
        }
    }

    void HandleFireTiming()
    {
        if (!isOnFire)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                isOnFire = true;
                fireTimer = 0f;
                fireDurationTimer = 0f;
                activeFireDuration = fireDuration;
                Debug.Log("🔥 You caught fire!");
            }
        }
        else
        {
            fireDurationTimer += Time.deltaTime;
            if (fireDurationTimer >= activeFireDuration)
            {
                isOnFire = false;
                Debug.Log("The fire went out on its own (you should have put it out!)");
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("You died!");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // Lets external things (lava, the F key) set the player on fire.
    // If a duration is given, the fire burns for that many seconds.
    public void CatchFire(float duration = -1f)
    {
        if (!isOnFire) Debug.Log("🔥 You've been set on fire!");
        isOnFire = true;
        fireDurationTimer = 0f;
        activeFireDuration = duration > 0f ? duration : fireDuration;
    }

    public void ExtinguishFire()
    {
        isOnFire = false;
        fireTimer = 0f;
        Debug.Log("💨 Fire extinguished!");
    }
}
