using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ForestersLodgeBuildingPanelOld : BuildingPanelOld
    {
        [SerializeField] private Toggle _harvestFruitsToggle;
        [SerializeField] private Toggle _cutFruitingToggle;

        private ForestersLodge _forestersLodge => buildingOld as ForestersLodge;

        public override void Init(BuildingOld buildingOld)
        {
            base.Init(buildingOld);
            Refresh();
        }

        private void Refresh()
        {
            _harvestFruitsToggle.isOn = _forestersLodge.HarvestFruits;
            _cutFruitingToggle.isOn = _forestersLodge.CutFruitingTrees;
        }

        public void SetWorkZonePressed()
        {
            Debug.Log("Build Me!");
            ZoneManager.Instance.PlanZone(ZoneType.Forester, buildingOld);
        }

        public void HarvestFruitsToggleChanged()
        {
            _forestersLodge.HarvestFruits = _harvestFruitsToggle.isOn;
        }
        
        public void CutFruitingToggleChanged()
        {
            _forestersLodge.CutFruitingTrees = _cutFruitingToggle.isOn;
        }
    }
}
