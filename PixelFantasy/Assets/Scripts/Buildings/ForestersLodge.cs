using System;
using Controllers;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Buildings
{
    public class ForestersLodge : ProductionBuildingOld
    {
        private bool _cutFruitingTrees;
        public bool CutFruitingTrees
        {
            get => _cutFruitingTrees;
            set
            {
                _cutFruitingTrees = value;
                if (AssignedZone is ForesterZone zone)
                {
                    zone.RefreshJobs();
                }
            }
        }
        
        private bool _harvestFruits;
        public bool HarvestFruits
        {
            get => _harvestFruits;
            set
            {
                _harvestFruits = value;
                if (AssignedZone is ForesterZone zone)
                {
                    zone.RefreshJobs();
                }
            }
        }
        
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }

        private float _refreshTimer;
        private void Update()
        {
            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= 5)
            {
                _refreshTimer = 0;
                if (AssignedZone is ForesterZone zone)
                {
                    zone.RefreshJobs();
                }
            }
        }
    }
}
