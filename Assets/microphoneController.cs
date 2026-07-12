using UnityEngine;

public class MicrophoneController : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public float blowThreshold = 0.05f;  // blow sensitivity
    public float currentVolume = 0f;

    private AudioClip micClip;
    private string micDevice;
    private int sampleWindow = 128;

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }

        micDevice = Microphone.devices[0];
        Debug.Log("Using microphone: " + micDevice);

        micClip = Microphone.Start(micDevice, true, 10, 44100);
    }

    void Update()
    {
        currentVolume = GetMicVolume();

        if (currentVolume > blowThreshold)
        {
            Debug.Log("💨 Blow detected! Volume: " + currentVolume);
            if (playerHealth != null && playerHealth.isOnFire)
            {
                playerHealth.ExtinguishFire();
            }
        }
    }

    float GetMicVolume()
    {
        if (micClip == null) return 0f;

        int micPosition = Microphone.GetPosition(micDevice) - sampleWindow;
        if (micPosition < 0) return 0f;

        float[] samples = new float[sampleWindow];
        micClip.GetData(samples, micPosition);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / sampleWindow); // RMS = average volume
    }
}
