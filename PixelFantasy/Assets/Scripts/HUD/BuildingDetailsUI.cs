using System;
using Buildings;
using Buildings.Building_Panels;
using UnityEngine;

namespace HUD
{
    public class BuildingDetailsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;

        private Building _building;
        private bool _isInsideBuilding;
        private BuildingPanel _buildingPanel;

        private void Start()
        {
            _root.SetActive(false);
        }

        public void Show(Building building)
        {
            _building = building;
            _buildingPanel = Instantiate(_building.BuildingPanel, _root.transform);
            _buildingPanel.Init(building);
            
            _root.SetActive(true);
        }

        public void Hide()
        {
            _building = null;
            _root.SetActive(false);
            Destroy(_buildingPanel.gameObject);
            _buildingPanel = null;
        }

        public void ChangeViewPressed()
        {
            if (_isInsideBuilding)
            {
                _isInsideBuilding = false;
                _building.ViewExterior();
            }
            else
            {
                _isInsideBuilding = true;
                _building.ViewInterior();
            }
        }

        public void DeconstructPressed()
        {
            // TODO: Build me!
        }
    }
}
