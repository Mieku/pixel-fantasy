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
        [SerializeField] private Image _zoneIcon, _buildIcon, _manageIcon;
        [SerializeField] private TextMeshProUGUI _zoneName;
        [SerializeField] private TooltipTrigger _warningIcon;

        private Zone _zone;

        public void Init(Zone zone)
        {
            _zoneIcon.sprite = zone.ZoneTypeData.Icon;
            _zoneName.text = zone.Name;
            _warningIcon.gameObject.SetActive(false);
            
            SetColour(zone.ZoneTypeData.Colour);
            
            // TODO: Remove this
            ShowWarning("Tester warning!!");
        }

        private void SetColour(Color colour)
        {
            _zoneIcon.color = colour;
            _buildIcon.color = colour;
            _manageIcon.color = colour;
            _zoneName.color = colour;
        }

        public void ShowWarning(string warning)
        {
            _warningIcon.Content = warning;
            _warningIcon.gameObject.SetActive(true);
        }

        public void HideWarning()
        {
            _warningIcon.gameObject.SetActive(false);
        }

        public void BuildBtnPressed()
        {
            // TODO: Build Me
            Debug.LogError("Not Built Yet");
        }

        public void ManageBtnPressed()
        {
            // TODO: Build Me
            Debug.LogError("Not Built Yet");
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
