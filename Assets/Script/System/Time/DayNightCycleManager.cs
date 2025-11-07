using UnityEngine;
using UnityEngine.UI; // Use for UI Image overlay
using System.Collections;

public class DayNightCycleManager : MonoBehaviour
{
    public Image overlayImage; // Reference to your UI Image overlay
    public float secondsPerDay = 120f; // Duration of one full day/night cycle in real-time seconds

    [Header("Day-Night Colors")]
    public Color dayColor = new Color(1f, 1f, 1f, 0f); // White/clear overlay for day
    public Color nightColor = new Color(0f, 0f, 0f, 0.8f); // Dark/translucent overlay for night
    public Color sunriseColor = new Color(1f, 0.6f, 0f, 0.4f); // Orange tint for sunrise
    public Color sunsetColor = new Color(1f, 0.4f, 0f, 0.6f); // Red/orange tint for sunset

    private float currentTimeOfDay = 0f; // Range from 0 to 1
    private float timeMultiplier = 1f;

    void Start()
    {
        if (overlayImage == null)
        {
            Debug.LogError("Overlay Image reference not set!");
            enabled = false;
            return;
        }
        timeMultiplier = 24f / (secondsPerDay / 3600f); // Adjust time multiplier for smooth transition calculation
    }

    void Update()
    {
        // Update time
        currentTimeOfDay += Time.deltaTime / secondsPerDay;
        if (currentTimeOfDay >= 1f)
        {
            currentTimeOfDay = 0f; // Reset to start of day
        }

        // Update the overlay color
        UpdateDayNightOverlay();
    }

    void UpdateDayNightOverlay()
    {
        Color targetColor;

        if (currentTimeOfDay < 0.25f) // Morning (0 to 0.25)
        {
            targetColor = Color.Lerp(nightColor, sunriseColor, currentTimeOfDay / 0.25f);
        }
        else if (currentTimeOfDay < 0.5f) // Day (0.25 to 0.5)
        {
            targetColor = Color.Lerp(sunriseColor, dayColor, (currentTimeOfDay - 0.25f) / 0.25f);
        }
        else if (currentTimeOfDay < 0.75f) // Evening (0.5 to 0.75)
        {
            targetColor = Color.Lerp(dayColor, sunsetColor, (currentTimeOfDay - 0.5f) / 0.25f);
        }
        else // Night (0.75 to 1)
        {
            targetColor = Color.Lerp(sunsetColor, nightColor, (currentTimeOfDay - 0.75f) / 0.25f);
        }

        overlayImage.color = targetColor;
    }
}
