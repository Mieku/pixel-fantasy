using System.Collections.Generic;
using Buildings;
using Controllers;
using Gods;
using Items;
using Popups.Zone_Popups;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class ForesterZone : Zone
    {
        private ForestersLodge _building;
        private List<Resource> _resources = new List<Resource>();
        private bool _harvestFruits => _building.HarvestFruits;
        private bool _cutFruitingTrees => _building.CutFruitingTrees;
        
        public override ZoneType ZoneType => ZoneType.Forester;

        private Command _harvestCmd, _cutPlantCmd;
        
        public ForesterZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile, ForestersLodge building) : base(uid, gridPositions, layeredRuleTile, building)
        {
            _building = building;
            _building.AssignedZone = this;
            
            Init();
        }

        private void Init()
        {
            _harvestCmd = Librarian.Instance.GetCommand("Harvest Resource");
            _cutPlantCmd = Librarian.Instance.GetCommand("Extract Resource");
            _resources = FindAllResources();
        }

        private List<Resource> FindAllResources()
        {
            return Helper.GetAllGenericOnGridPositions<Resource>(WorldPositions);
        }

        public void RefreshJobs()
        {
            foreach (var resource in _resources)
            {
                if (resource is GrowingResource growingResource)
                {
                    if (!_harvestFruits && growingResource.IsPending(_harvestCmd))
                    {
                        growingResource.CancelPending();
                    }
                    
                    if (!_cutFruitingTrees && growingResource.IsFruiting && growingResource.IsPending(_cutPlantCmd))
                    {
                        growingResource.CancelPending();
                    }
                    
                    
                    if (_harvestFruits && growingResource.HasFruitAvailable)
                    {
                        growingResource.CreateTask(_harvestCmd);
                    }

                    if (_cutFruitingTrees && growingResource.FullyGrown)
                    {
                        growingResource.CreateTask(_cutPlantCmd);
                    } 
                    else if (!_cutFruitingTrees && growingResource.FullyGrown && !growingResource.IsFruiting)
                    {
                        growingResource.CreateTask(_cutPlantCmd);
                    }
                }
            }
        }

        protected override void AssignName()
        {
            Name = ZoneTypeData.ZoneTypeName;
        }

        public override void ClickZone()
        {
            base.ClickZone();
            
            HUDController.Instance.ShowBuildingDetails(_building);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();

            if (HomePopup.Instance != null)
            {
                HomePopup.Hide();
            }
        }
    }
}
