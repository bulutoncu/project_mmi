using UnityEngine;

// Fire controls (multimodal fusion):
// - While burning the player runs faster (risk/reward: pay health, gain speed)
// - F: set yourself on fire on purpose
// - Typing the word "blow" extinguishes the fire
//   (keyboard alternative to blowing into the microphone)
// MazeGenerator adds this to the player automatically.
public class FireControls : MonoBehaviour
{
    [Header("Ignite")]
    public KeyCode igniteKey = KeyCode.F;
    public float selfIgniteBurnDuration = 5f;   // seconds of burning after pressing F
    public float burningSpeedMultiplier = 1.5f; // movement speed multiplier while on fire

    [Header("Extinguish by typing")]
    public string extinguishWord = "blow";
    public float typingTimeout = 1.2f;          // max seconds between letters

    PlayerHealth health;
    PlayerMovement movement;
    MicrophoneController mic;
    float normalSpeed;
    string typed = "";
    float lastLetterTime = -99f;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();
        if (movement != null) normalSpeed = movement.speed;
    }

    // Is the player currently blowing into the microphone?
    // Extinguishing always wins over igniting, so F is ignored while blowing.
    bool IsBlowing()
    {
        if (mic == null) mic = FindFirstObjectByType<MicrophoneController>();
        return mic != null && mic.currentVolume > mic.blowThreshold;
    }

    void Update()
    {
        if (health == null) return;

        // Speed up while burning, back to normal once the fire is out
        if (movement != null)
            movement.speed = health.isOnFire ? normalSpeed * burningSpeedMultiplier : normalSpeed;

        // F: ignite on purpose — unless the player is blowing right now
        // (extinguish intent has priority over ignite)
        if (Input.GetKeyDown(igniteKey) && !health.isOnFire)
        {
            if (IsBlowing())
            {
                Debug.Log("💨 You're blowing — the fire can't catch!");
            }
            else
            {
                health.CatchFire(selfIgniteBurnDuration);
                Debug.Log("🔥 You set yourself on fire — speed up, health down!");
            }
        }

        ReadTypedLetters();
    }

    // Watches the keyboard; typing the extinguish word puts the fire out
    void ReadTypedLetters()
    {
        foreach (char c in Input.inputString)
        {
            if (!char.IsLetter(c)) continue;

            if (Time.time - lastLetterTime > typingTimeout)
                typed = "";   // typed too slowly, start over

            typed += char.ToLowerInvariant(c);
            lastLetterTime = Time.time;

            if (typed.EndsWith(extinguishWord))
            {
                typed = "";
                if (health.isOnFire)
                {
                    health.ExtinguishFire();
                    Debug.Log("⌨️ You typed \"" + extinguishWord + "\" and put the fire out!");
                }
            }
            else if (typed.Length > 24)
            {
                // keep the buffer short; only the tail can still match
                typed = typed.Substring(typed.Length - extinguishWord.Length);
            }
        }
    }
}
