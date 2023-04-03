using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using TMPro;
using UnityEngine;

public class WorkerEntry : MonoBehaviour
{
    [SerializeField] private GameObject _chooseEntryHandle;
    [SerializeField] private GameObject _choosenEntryHandle;
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TextMeshProUGUI _workerName;
    [SerializeField] private GameObject _selectBtn;

    private UnitState _chosenUnit;
    private BuildingPanel _panel;

    private void Awake()
    {
        GameEvents.OnUnitOccupationChanged += GameEvent_OnUnitOccupationChanged;
    }

    private void OnDestroy()
    {
        GameEvents.OnUnitOccupationChanged -= GameEvent_OnUnitOccupationChanged;
    }

    public void Init(UnitState unitState, BuildingPanel panel)
    {
        _panel = panel;
        _chosenUnit = unitState;
        Refresh();
    }

    private void Refresh()
    {
        if (_chosenUnit != null)
        {
            _choosenEntryHandle.SetActive(true);
            _chooseEntryHandle.SetActive(false);
            _workerName.text = _chosenUnit.FullName;
        }
        else
        {
            _choosenEntryHandle.SetActive(false);
            _chooseEntryHandle.SetActive(true);
            _selectBtn.SetActive(false);
            
            var availableWorkers = UnitsManager.Instance.UnemployedUnits;
            _dropdown.options.Clear();
            _dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
            _dropdown.SetValueWithoutNotify(0);
            foreach (var availableWorker in availableWorkers)
            {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(availableWorker.FullName));
            }
        }
    }

    public void OnDropdownSelectChanged(TMP_Dropdown dropdown)
    {
        var value = dropdown.options[dropdown.value].text;
        if (value == "Empty")
        {
            _selectBtn.SetActive(false);
        }
        else
        {
            _selectBtn.SetActive(true);
        }
    }

    public void AssignWorkerPressed()
    {
        var value = _dropdown.options[_dropdown.value].text;
        var worker = UnitsManager.Instance.GetUnit(value);
        _panel.Building.AssignWorker(worker);
        _chosenUnit = worker;
        Refresh();
        GameEvents.Trigger_OnUnitOccupationChanged(_chosenUnit);
    }

    public void RemoveWorkerPressed()
    {
        var prevWorker = _chosenUnit;
        _panel.Building.UnassignWorker(_chosenUnit);
        _chosenUnit = null;
        Refresh();
        GameEvents.Trigger_OnUnitOccupationChanged(prevWorker);
    }

    private void GameEvent_OnUnitOccupationChanged(UnitState unit)
    {
        Refresh();
    }
}
