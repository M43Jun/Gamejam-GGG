using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public event EventHandler<TimeSpan> WorldTimeChanged;

        [SerializeField]
        private float dayLength = 15f; 

        private TimeSpan currentTime;
        private float minuteLength => dayLength / WorldTimeConstants.MinutesInDay;

        void Start()
        {
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            currentTime += TimeSpan.FromMinutes(0.1);

            if (currentTime.TotalMinutes >= WorldTimeConstants.MinutesInDay)
            {
                currentTime = TimeSpan.FromMinutes(currentTime.TotalMinutes % WorldTimeConstants.MinutesInDay);
            }

            WorldTimeChanged?.Invoke(this, currentTime);

            yield return new WaitForSeconds(minuteLength);
            StartCoroutine(AddMinute());
        }
    }
}