using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingScheduleHourDisplay : MonoBehaviour
    {
        [SerializeField] private Image _colourDisplay;
        [SerializeField] private KinlingScheduleDisplay _kinlingSchedule;

        public int Hour;
        
        public void OnClicked()
        {
            _kinlingSchedule.OnHourClicked(Hour, this);
        }

        public void SetColour(Color color)
        {
            _colourDisplay.color = color;
        }
    }
}
