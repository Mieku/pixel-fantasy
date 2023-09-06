using Characters;
using Managers;
using Popups;
using Popups.Kinling_Info_Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class KinlingDetailsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _currentAction;
        [SerializeField] private Image _jobIcon;
        [SerializeField] private TextMeshProUGUI _jobName;
        [SerializeField] private Image _jobBarFill;
        [SerializeField] private GameObject _root;

        private Unit _unit;
        
        private void Start()
        {
            _root.SetActive(false);
        }

        public void Show(Unit unit)
        {
            _unit = unit;
            _root.SetActive(true);
            
            _unitName.text = unit.GetUnitState().FullName;
            _jobName.text = unit.GetUnitState().CurrentJob.JobNameWithTitle;
            if (_unit.GetUnitState().CurrentJob.JobData.JobIcon != null)
            {
                _jobIcon.sprite = _unit.GetUnitState().CurrentJob.JobData.JobIcon;
            }
            else
            {
                _jobIcon.sprite = Librarian.Instance.GetSprite("Question Mark");
            }
        }

        public void Hide()
        {
            _unit = null;
            _root.SetActive(false);
        }
        
        public void ShowKinlingInfoPopupPressed()
        {
            KinlingInfoPopup.Show(_unit);
        }
    }
}
