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

        private IZone _zone;

        public void Init(IZone zone)
        {
            _zone = zone;
            
            Refresh();
        }

        public void Refresh()
        {
            _zoneIcon.sprite = _zone.ZoneTypeData.Icon;
            _zoneName.text = _zone.Name;

            // For font colour, take the zone colour and shift darker
            var darkerColour = _zone.ZoneTypeData.Colour;
            darkerColour.r -= (darkerColour.r * .30f);
            darkerColour.g -= (darkerColour.g * .30f);
            darkerColour.b -= (darkerColour.b * .30f);
            
            SetColour(darkerColour);
        }

        public void SetColour(Color colour)
        {
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
