using System;
using HUD.Tooltip;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zones;

namespace HUD
{
    public class ZonePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Image _zoneIcon;
        [SerializeField] private TextMeshProUGUI _zoneName;

        private Zone _zone;

        public void Init(IZone zone)
        {
            _zoneIcon.sprite = zone.ZoneTypeData.Icon;
            _zoneName.text = zone.Name;
            
            SetColour(zone.ZoneTypeData.Colour);
        }

        public void SetColour(Color colour)
        {
            _zoneIcon.color = colour;
            _zoneName.color = colour;
        }

        private void Awake()
        {
            GameEvents.OnZoneDisplayChanged += GameEvents_OnZoneDisplayChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnZoneDisplayChanged -= GameEvents_OnZoneDisplayChanged;
        }
        
        private void GameEvents_OnZoneDisplayChanged(bool zonesVisible)
        {
            _panelRoot.SetActive(zonesVisible);
        }
    }
}
