using System;
using FunkyCode;
using FunkyCode.Utilities;
using Managers;
using Systems.Buildings.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Lighting.Scripts
{
    [RequireComponent(typeof(Light2D))]
    public class LightSource : MonoBehaviour
    {
        public bool ShouldFlicker;
        
        private float flickersPerSecond = 15f;
        private float flickerRangeMin = -0.1f;
        private float flickerRangeMax = 0.1f;
        float lightAlpha;
        private float timer;
        
        private Light2D _light2D;

        private void Awake()
        {
            _light2D = GetComponent<Light2D>();
            lightAlpha = _light2D.color.a;
        }

        private void Update()
        {
            HandleFlicker();
        }

        private void HandleFlicker()
        {
            if(!ShouldFlicker) return;
            
            timer += TimeManager.Instance.DeltaTime;

            if (timer > 1 / flickersPerSecond)
            {
                float tempAlpha = lightAlpha;
                tempAlpha = tempAlpha + Random.Range(flickerRangeMin, flickerRangeMax);
                _light2D.color.a = tempAlpha;
                timer = 0;
            }
        }

        public void SetLightOn(bool lightOn)
        {
            _light2D.enabled = lightOn;
        }

        public bool GetLightOn()
        {
            return _light2D.enabled;
        }
    }
}
