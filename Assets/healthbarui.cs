using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider slider;

    void Update()
    {
        slider.value = playerHealth.currentHealth;
    }
}