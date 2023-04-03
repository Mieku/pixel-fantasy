using System.Collections;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerPanel : MonoBehaviour
{
    [SerializeField] private WorkerEntry _workerEntryPrefab;
    [SerializeField] private Transform _layout;

    private List<WorkerEntry> _displayedEntrys = new List<WorkerEntry>();
    private BuildingPanel _buildingPanel;

    public void Init(BuildingPanel buildingPanel)
    {
        _workerEntryPrefab.gameObject.SetActive(false);
        _buildingPanel = buildingPanel;
        RefreshEntries();
    }

    // Triggered by dropdown
    public void AssignWorker(TMP_Dropdown dropdown)
    {
        // var value = dropdown.options[dropdown.value].text;
        //
        // // Check all the dropdowns and get a list of current chosen, find outlier and unassign
        // List<string> allValues = new List<string>();
        // foreach (var dropdownEntry in _displayedEntrys)
        // {
        //     allValues.Add(dropdownEntry.options[dropdown.value].text);
        // }
        // var curWorkers = new List<UnitState>(_buildingPanel.Building.Occupants);
        // foreach (var curWorker in curWorkers)
        // {
        //     if (allValues.Contains(curWorker.FullName))
        //     {
        //         curWorkers.Remove(curWorker);
        //     }
        // }
        //
        // foreach (var curWorker in curWorkers)
        // {
        //     _buildingPanel.Building.UnassignWorker(curWorker);
        // }
        //
        // if (value != "Empty")
        // {
        //     var worker = UnitsManager.Instance.GetUnit(value);
        //     _buildingPanel.Building.AssignWorker(worker);
        // }
        //
        // RefreshEntries();
    }

    private void RefreshEntries()
    {
        var occupants = _buildingPanel.Building.Occupants;
        var maxOccupants = _buildingPanel.Building.MaxOccupants;
        
        ClearEntries();
        
        for (int i = 0; i < maxOccupants; i++)
        {
            var entry = Instantiate(_workerEntryPrefab, _layout);
            entry.gameObject.SetActive(true);
        
            if (occupants.Count > i)
            {
                var occupant = occupants[i];
                entry.Init(occupant, _buildingPanel);
            }
            else
            {
                entry.Init(null, _buildingPanel);
            }
            
            _displayedEntrys.Add(entry);
        }
    }

    private void ClearEntries()
    {
        foreach (var entry in _displayedEntrys)
        {
            Destroy(entry.gameObject);
        }
        _displayedEntrys.Clear();
    }
}
