using System;
using Managers;
using TMPro;
using UnityEngine;

namespace HUD
{
    public class ClockHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _clockRenderer;

        private void Refresh()
        {
            var time = EnvironmentManager.Instance.GameTime.Readable();
            _clockRenderer.text = time;
        }
        
        private void FixedUpdate()
        {
            Refresh();
        }
    }
}
