using System.Collections.Generic;
using Characters;
using TMPro;
using UnityEngine;

namespace Systems.Details.Building_Details.Scripts
{
    public class OccupantsBuildingPanel : BuildingPanel
    {
        [SerializeField] private TextMeshProUGUI _panelNameText;
        [SerializeField] private BuildingOccupantSlot _slotPrefab;
        [SerializeField] private Transform _slotParent;

        private List<BuildingOccupantSlot> _displayedSlots = new List<BuildingOccupantSlot>();
        
        protected override void Show()
        {
            _panelNameText.text = _building.OccupantAdjective;
            Refresh();
        }

        protected override void Refresh()
        {
            ClearSlots();
            int maxOccupants = _building.BuildingData.MaxOccupants;
            var occupants = _building.GetOccupants();
            int emptySlots = maxOccupants - occupants.Count;
            foreach (var occupant in occupants)
            {
                var slot = Instantiate(_slotPrefab, _slotParent);
                slot.Init(occupant, _building, OnKinlingSelected);
                _displayedSlots.Add(slot);
            }

            for (int i = 0; i < emptySlots; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotParent);
                slot.Init(null, _building, OnKinlingSelected);
                _displayedSlots.Add(slot);
            }
        }

        private void ClearSlots()
        {
            foreach (var displayedSlot in _displayedSlots)
            {
                Destroy(displayedSlot.gameObject);
            }
            _displayedSlots.Clear();
        }

        private void OnKinlingSelected(Kinling selectedKinling, Kinling previousKinling)
        {
            if (previousKinling != null)
            {
                _building.RemoveOccupant(previousKinling);
            }

            if (selectedKinling != null)
            {
                _building.AddOccupant(selectedKinling);
            }
            
            Refresh();
        }
    }
}
