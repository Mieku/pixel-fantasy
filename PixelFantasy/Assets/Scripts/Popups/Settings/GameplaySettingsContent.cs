using Controllers;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Settings
{
    public class GameplaySettingsContent : SettingsContent
    {
        [SerializeField] private TMP_Dropdown _cameraSpeedDD;
        [SerializeField] private Toggle _edgeScrollingEnabled;
        [SerializeField] private Toggle _outlineHovered;
        
        public override void OnShow()
        {
            _cameraSpeedDD.SetValueWithoutNotify((int) PlayerSettings.CameraSpeed);
            _edgeScrollingEnabled.SetIsOnWithoutNotify(PlayerSettings.EdgeScrollingEnabled);
            _outlineHovered.SetIsOnWithoutNotify(PlayerSettings.OutlineOnHover);
        }
        
        public void OnCameraSpeedDDChanged(int value)
        {
            var enumValue = (ECameraScrollSpeed) value;
            PlayerSettings.CameraSpeed = enumValue;
        }

        public void OnEdgeScrollingEnabledToggle(bool value)
        {
            PlayerSettings.EdgeScrollingEnabled = value;
        }
        
        public void OnOutlineHoveredToggle(bool value)
        {
            PlayerSettings.OutlineOnHover = value;
        }
    }
}
