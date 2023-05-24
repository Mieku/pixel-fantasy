using Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingScheduleOption : MonoBehaviour
    {
        public ScheduleOption Option;
        
        [SerializeField] private KinlingScheduleDisplay _display;
        [SerializeField] private Sprite _unselected, _selected;
        [SerializeField] private Image _panel;

        public void OnClicked()
        {
            _display.OptionSelected(this);
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected)
            {
                _panel.sprite = _selected;
            }
            else
            {
                _panel.sprite = _unselected;
            }
        }
    }
}
