using System;
using Buildings;
using Buildings.Building_Panels;
using UnityEngine;

namespace HUD
{
    public class BuildingDetailsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;

        private BuildingOld buildingOld;
        private bool _isInsideBuilding;
        private BuildingPanel _buildingPanel;

        private void Start()
        {
            _root.SetActive(false);
        }

        public void Show(BuildingOld buildingOld)
        {
            this.buildingOld = buildingOld;
            _buildingPanel = Instantiate(this.buildingOld.BuildingPanel, _root.transform);
            _buildingPanel.Init(buildingOld);
            
            _root.SetActive(true);
        }

        public void Hide()
        {
            if (_buildingPanel == null) return;
                
            buildingOld = null;
            _root.SetActive(false);
            Destroy(_buildingPanel.gameObject);
            _buildingPanel = null;
        }

        public void ChangeViewPressed()
        {
            if (_isInsideBuilding)
            {
                _isInsideBuilding = false;
                buildingOld.ViewExterior();
            }
            else
            {
                _isInsideBuilding = true;
                buildingOld.ViewInterior();
            }
        }

        public void DeconstructPressed()
        {
            // TODO: Build me!
        }
    }
}
