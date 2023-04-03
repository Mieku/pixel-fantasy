using Gods;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ForestersLodgeBuildingPanel : BuildingPanel
    {
        [SerializeField] private Toggle _harvestFruitsToggle;
        [SerializeField] private Toggle _cutFruitingToggle;

        private ForestersLodge _forestersLodge => Building as ForestersLodge;

        public override void Init(Building building)
        {
            base.Init(building);
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
            ZoneManager.Instance.PlanZone(ZoneType.Forester, Building);
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
