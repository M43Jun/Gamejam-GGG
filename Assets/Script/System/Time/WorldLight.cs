using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    public class WorldLight : MonoBehaviour
    {
        private Light2D light;

        [SerializeField]
        private WorldTime worldTime;

        [SerializeField]
        private Gradient gradient;

        private void Awake()
        {
            light = GetComponent<Light2D>();
            worldTime.WorldTimeChanged += OnWorldTimeChanged;
        }

        private void OnDestroy()
        {
            worldTime.WorldTimeChanged -= OnWorldTimeChanged;
        }

        private void OnWorldTimeChanged(object sender, TimeSpan newtime)
        {
            float percentOfDay = PercentOfDay(newtime);
            light.color = gradient.Evaluate(percentOfDay);

            // Debug untuk melihat nilai
            Debug.Log($"Time: {newtime:hh\\:mm}, Percent: {percentOfDay:F3}, Color: {light.color}");
        }

        private float PercentOfDay(TimeSpan timeSpan)
        {
            float totalMinutes = (float)timeSpan.TotalMinutes % WorldTimeConstants.MinutesInDay;
            return totalMinutes / WorldTimeConstants.MinutesInDay;
        }
    }
}