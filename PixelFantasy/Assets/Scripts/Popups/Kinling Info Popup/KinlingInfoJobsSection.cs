using Characters;
using Managers;
using TMPro;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoJobsSection : MonoBehaviour
    {
        [SerializeField] private KinlingScheduleDisplay _kinlingSchedule;
        
        private Kinling _kinling;

        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            
            RefreshSchedule();
        }

        private void RefreshSchedule()
        {
            _kinlingSchedule.Refresh(_kinling);
        }
    }
}
