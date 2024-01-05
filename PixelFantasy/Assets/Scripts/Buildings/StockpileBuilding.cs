using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using UnityEngine;

namespace Buildings
{
    public class StockpileBuilding : Building
    {
        private StockpileBuildingData _stockpileBuildingData => _buildingData as StockpileBuildingData;
        private List<ItemData> _unallowedItems = new List<ItemData>();
        public override BuildingType BuildingType => BuildingType.Stockpile;
        
        public override string OccupantAdjective => "Workers";
        
        public bool IsItemStockpileAllowed(ItemData itemData)
        {
            return !_unallowedItems.Contains(itemData);
        }

        public void SetAllowedStockpileItem(ItemData itemData, bool isAllowed)
        {
            if (isAllowed)
            {
                if (_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Remove(itemData);
                }
            }
            else
            {
                if (!_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Add(itemData);
                }
            }
        }
        
        public override List<Unit> GetPotentialOccupants()
        {
            var relevantAbilites = _stockpileBuildingData.RelevantAbilityTypes;
            
            var unemployed = UnitsManager.Instance.UnemployedKinlings;
            List<Unit> sortedKinlings = unemployed
                .OrderByDescending(kinling => kinling.GetUnitState().RelevantAbilityScore(relevantAbilites)).ToList();
            return sortedKinlings;
        }
        
        protected override bool CheckForIssues()
        {
            bool hasIssue = base.CheckForIssues();

            // Check if has no workers
            if (_state == BuildingState.Built && GetOccupants().Count == 0)
            {
                hasIssue = true;
                var noWorkersNote = _buildingNotes.Find(note => note.ID == "No Workers");
                if (noWorkersNote == null)
                {
                    _buildingNotes.Add(new BuildingNote("There are no workers!", false, "No Workers"));
                }
            }
            else
            {
                var noWorkersNote = _buildingNotes.Find(note => note.ID == "No Workers");
                if (noWorkersNote != null)
                {
                    _buildingNotes.Remove(noWorkersNote);
                }
            }


            if (hasIssue)
            {
                _buildingNotification.SetNotification(BuildingNotification.ENotificationType.Issue);
            }
            else
            {
                _buildingNotification.SetNotification(BuildingNotification.ENotificationType.None);
            }

            return hasIssue;
        }
    }
}
