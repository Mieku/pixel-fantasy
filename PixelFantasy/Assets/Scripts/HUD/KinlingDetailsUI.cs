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
        [SerializeField] private GameObject _root;

        private Kinling _kinling;
        
        private void Start()
        {
            _root.SetActive(false);
        }

        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            _root.SetActive(true);
            
            _unitName.text = kinling.FullName;
        }

        public void Hide()
        {
            _kinling = null;
            _root.SetActive(false);
        }
        
        public void ShowKinlingInfoPopupPressed()
        {
            KinlingInfoPopup.Show(_kinling);
        }
    }
}
