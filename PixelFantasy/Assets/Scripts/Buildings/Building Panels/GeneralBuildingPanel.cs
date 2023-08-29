using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Buildings.Building_Panels
{
    public class GeneralBuildingPanel : MonoBehaviour
    {
        [Header("Building Notes")] 
        [SerializeField] private TextMeshProUGUI _notesText;
        [SerializeField] private Color _notesPositiveColour;
        [SerializeField] private Color _notesNegativeColour;
        [SerializeField] private int _maxNotes = 4;

        [Header("Occupants")] 
        [SerializeField] private BuildingOccupant _occupantPrefab;
        [SerializeField] private Transform _occupantParent;
        [SerializeField] private SelectKinlingSidePanel _selectKinlingSidePanel;
        private List<BuildingOccupant> _displayedOccupants = new List<BuildingOccupant>();

        [Header("Inventory")] 
        [SerializeField] private BuildingInventory _inventory;
        
        private Building _building;

        public void Init(Building building)
        {
            gameObject.SetActive(true);
            _building = building;
            RefreshNotes();
            RefreshOccupants();
            RefreshInventory();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _inventory.Close();
        }

        private void RefreshNotes()
        {
            _notesText.text = "";
            var notes = _building.BuildingNotes;
            int noteIndex = 1;
            foreach (var note in notes)
            {
                if (noteIndex > _maxNotes)
                {
                    return;
                }

                string colorTag = $"<color=#{ColorUtility.ToHtmlStringRGB(_notesNegativeColour)}>";
                if (note.IsPositive)
                {
                    colorTag = $"<color=#{ColorUtility.ToHtmlStringRGB(_notesPositiveColour)}>";
                }
                
                _notesText.text += $"{colorTag}- {note.Note}</color>\n";
                noteIndex++;
            }
        }

        private void RefreshOccupants()
        {
            foreach (var displayedOccupant in _displayedOccupants)
            {
                Destroy(displayedOccupant.gameObject);
            }
            _displayedOccupants.Clear();
            
            int maxOccupants = _building.BuildingData.MaxOccupants;
            var occupants = _building.GetOccupants();

            foreach (var occupant in occupants)
            {
                var buildingOccupant = Instantiate(_occupantPrefab, _occupantParent);
                buildingOccupant.Init(occupant, OnOccupantPressed);
                _displayedOccupants.Add(buildingOccupant);
            }

            var remaining = maxOccupants - occupants.Count;
            for (int i = 0; i < remaining; i++)
            {
                var buildingOccupant = Instantiate(_occupantPrefab, _occupantParent);
                buildingOccupant.Init(null, OnOccupantPressed);
                _displayedOccupants.Add(buildingOccupant);
            }
        }

        private void OnOccupantPressed(Unit unit)
        {
            OpenSelectKinlingPanel(unit);
        }

        // Chosen Unit can be null
        private void OnOccupantChosen(Unit unit, Unit originalUnit)
        {
            _selectKinlingSidePanel.gameObject.SetActive(false);
            if (unit != null)
            {
                _building.AddOccupant(unit);
                if (_building.BuildingData.BuildingType == BuildingType.Home)
                {
                    unit.GetUnitState().AssignedHome = _building;
                }
                else
                {
                    unit.GetUnitState().AssignedWorkplace = _building;
                }
                
                RefreshOccupants();
            }

            // For Swapping if needed
            if (originalUnit != null)
            {
                _building.RemoveOccupant(originalUnit);
                if (_building.BuildingData.BuildingType == BuildingType.Home)
                {
                    originalUnit.GetUnitState().AssignedHome = null;
                }
                else
                {
                    originalUnit.GetUnitState().AssignedWorkplace = null;
                }
                
                RefreshOccupants();
            }

            // Remove them
            if (unit == originalUnit)
            {
                _building.RemoveOccupant(unit);
                if (_building.BuildingData.BuildingType == BuildingType.Home)
                {
                    unit.GetUnitState().AssignedHome = null;
                }
                else
                {
                    unit.GetUnitState().AssignedWorkplace = null;
                }
                
                RefreshOccupants();
            }
        }

        private void OpenSelectKinlingPanel(Unit unit)
        {
            _selectKinlingSidePanel.gameObject.SetActive(true);
            _selectKinlingSidePanel.Init(_building, unit, OnOccupantChosen);
        }

        private void RefreshInventory()
        {
            _inventory.Open(_building);
            // _inventory.Refresh(_building);
        }
    }
}
