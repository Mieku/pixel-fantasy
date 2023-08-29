using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Buildings.Building_Panels
{
    public class SelectKinlingSidePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _noneAvailableMsg;
        [SerializeField] private BuildingKinlingSelect _kinlingSelectPrefab;
        [SerializeField] private Transform _kinlingSelectParent;
        
        private Building _building;
        private Unit _currentUnit;
        private Action<Unit, Unit> _kinlingSelectedCallback;
        private List<BuildingKinlingSelect> _displayedSelectors = new List<BuildingKinlingSelect>();
        
        public void Init(Building building, Unit currentUnit, Action<Unit, Unit> kinlingSelectedCallback)
        {
            _building = building;
            _currentUnit = currentUnit;
            _kinlingSelectedCallback = kinlingSelectedCallback;
            
            DisplayAvailableKinlings();
        }

        private void DisplayAvailableKinlings()
        {
            RemoveCurrentlyDisplayedSelectors();
            
            if (_currentUnit != null)
            {
                CreateKinlingSelector(_currentUnit);
            }

            if (_building.BuildingData.BuildingType == BuildingType.Home)
            {
                var homelessKinlings = UnitsManager.Instance.HomelessKinlings;
                foreach (var homelessKinling in homelessKinlings)
                {
                    CreateKinlingSelector(homelessKinling);
                }
            }
            else
            {
                var unemployedKinlings = UnitsManager.Instance.UnemployedKinlings;
                foreach (var unemployedKinling in unemployedKinlings)
                {
                    CreateKinlingSelector(unemployedKinling);
                }
            }
            
            // Show no kinling available msg if there are none
            _noneAvailableMsg.SetActive(_displayedSelectors.Count == 0);
        }

        private void RemoveCurrentlyDisplayedSelectors()
        {
            foreach (var displayedSelector in _displayedSelectors)
            {
                Destroy(displayedSelector.gameObject);
            }
            _displayedSelectors.Clear();
        }

        private void CreateKinlingSelector(Unit unit)
        {
            var selector = Instantiate(_kinlingSelectPrefab, _kinlingSelectParent);
            _displayedSelectors.Add(selector);

            selector.Init(unit, OnKinlingSelected);
        }

        private void OnKinlingSelected(Unit selectedKinling)
        {
            _kinlingSelectedCallback.Invoke(selectedKinling, _currentUnit);
        }

        public void CloseBtnPressed()
        {
            _kinlingSelectedCallback.Invoke(null, _currentUnit);
        }
    }
}
