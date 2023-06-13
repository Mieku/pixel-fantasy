using Characters;
using Managers;
using TMPro;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoJobsSection : MonoBehaviour
    {
        [SerializeField] private KinlingScheduleDisplay _kinlingSchedule;
        
        private Unit _unit;

        public void Show(Unit unit)
        {
            _unit = unit;
            
            RefreshSchedule();
        }

        private void RefreshSchedule()
        {
            _kinlingSchedule.Refresh(_unit);
        }
    }
}
