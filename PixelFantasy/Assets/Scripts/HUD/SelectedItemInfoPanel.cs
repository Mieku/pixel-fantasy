using System;
using System.Collections.Generic;
using Characters;
using Controllers;
using Gods;
using Interfaces;
using Items;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _selectedItemNameGeneric;
        [SerializeField] private TextMeshProUGUI _detailsGeneric;
        [SerializeField] private GameObject _genericPanel;
        [SerializeField] private GameObject _unitPanel;

        [Header("Kinling Details")] 
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _currentAction;
        [SerializeField] private Image _jobIcon;
        [SerializeField] private TextMeshProUGUI _jobName;
        [SerializeField] private Image _jobBarFill;
        
        private SelectionData _selectionData;
        private Unit _unit;

        private bool _showUnitData;

        private void Start()
        {
            HideItemDetails();
        }

        public void ShowUnitDetails(Unit unit)
        {
            _unit = unit;
            
            _genericPanel.SetActive(false);
            _unitPanel.SetActive(true);

            _showUnitData = true;
        }

        private void Update()
        {
            if(_showUnitData && _unit != null)
            {
                RefreshUnitDetails(_unit);
            }
        }

        private void RefreshUnitDetails(Unit unit)
        {
            _unitName.text = unit.GetUnitState().FullName;

            // var curAction = unit.GetCurrentAction();
            // if (curAction == null)
            // {
            //     _currentAction.text = "Idle";
            // }
            // else
            // {
            //     _currentAction.text = curAction.Details;
            // }
        }

        public void ShowItemDetails(SelectionData selectionData)
        {
            _showUnitData = false;
            _selectionData = selectionData;
            RefreshDetails();
            _genericPanel.SetActive(true);
            _unitPanel.SetActive(false);
            HUDOrders.Instance.DisplayOrders(selectionData);
        }

        public void HideItemDetails()
        {
            _showUnitData = false;
            _selectionData = null;
            _genericPanel.SetActive(false);
            _unitPanel.SetActive(false);
            HUDOrders.Instance.HideOrders();
        }

        private void RefreshDetails()
        {
            _selectedItemNameGeneric.text = _selectionData.ItemName;
        }

        public void ShowKinlingInfoPopupPressed()
        {
            KinlingInfoPopup.Show(_unit);
        }
    }
}
