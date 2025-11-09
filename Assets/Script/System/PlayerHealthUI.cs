using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public PlayerHealth playerHealth;
    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;

    private float targetValue;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;
            targetValue = playerHealth.currentHealth;
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        targetValue = playerHealth.currentHealth;
        healthSlider.maxValue = playerHealth.maxHealth;

        // Smooth transition
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * smoothSpeed);
    }
}
