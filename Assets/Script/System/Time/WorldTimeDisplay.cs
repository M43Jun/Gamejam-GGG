using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WorldTime
{
    [RequireComponent(typeof(TMP_Text))]
    public class WorldTimeDisplay : MonoBehaviour
    {
        [SerializeField]
        private WorldTime worldTime;
        private TMP_Text text;
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            worldTime.WorldTimeChanged += OnWorldTimeChanged;

        }
        private void OnDestroy()
        {
            worldTime.WorldTimeChanged -= OnWorldTimeChanged;
        }
        private void OnWorldTimeChanged(object sender, TimeSpan newtime)
        {
            text.SetText(newtime.ToString(@"hh\:mm"));
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

