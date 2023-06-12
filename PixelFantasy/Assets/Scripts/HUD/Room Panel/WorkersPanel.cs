using System.Collections.Generic;
using Characters;
using Managers;
using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class WorkersPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _noOneAssignedGO;
        [SerializeField] private GameObject _assignWorkerPanel;
        [SerializeField] private Transform _assignWorkerRoot;
        [SerializeField] private GameObject _assignWorkerBtnGO;
        [SerializeField] private Transform _assignedWorkersRoot;
        [SerializeField] private AssignedWorkerDisplay _assignedWorkerDisplayPrefab;
        [SerializeField] private SelectWorkerDisplay _selectWorkerDisplayPrefab;
        
        private ProductionZone _zone;
        private List<AssignedWorkerDisplay> _displayedAssignedWorkers = new List<AssignedWorkerDisplay>();
        private List<SelectWorkerDisplay> _displayedSelectWorkers = new List<SelectWorkerDisplay>();

        public void Show(ProductionZone zone)
        {
            _zone = zone;
            gameObject.SetActive(true);
            Refresh();
            CloseAssignWorkerPressed();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            _noOneAssignedGO.SetActive(_zone.Occupants.Count == 0);

            if (_zone.Occupants.Count >= _zone.MaxOccupants)
            {
                _assignWorkerBtnGO.SetActive(false);
            }
            else
            {
                _assignWorkerBtnGO.SetActive(true);
            }
            
            RefreshDisplayedAssignedWorkers();
        }

        private void RefreshDisplayedAssignedWorkers()
        {
            foreach (var displayedAssignedWorker in _displayedAssignedWorkers)
            {
                Destroy(displayedAssignedWorker.gameObject);
            }
            _displayedAssignedWorkers.Clear();
            var occupants = _zone.Occupants;
            foreach (var occupant in occupants)
            {
                var workerDisplay = Instantiate(_assignedWorkerDisplayPrefab, _assignedWorkersRoot);
                workerDisplay.Init(occupant, RemoveWorker);
                _displayedAssignedWorkers.Add(workerDisplay);
            }
        }

        public void AssignWorkerPressed()
        {
            _assignWorkerPanel.SetActive(true);
            RefreshDisplayedSelectWorkers();
        }

        public void CloseAssignWorkerPressed()
        {
            _assignWorkerPanel.SetActive(false);
        }

        private void RefreshDisplayedSelectWorkers()
        {
            foreach (var displayedSelectWorker in _displayedSelectWorkers)
            {
                Destroy(displayedSelectWorker.gameObject);
            }
            _displayedSelectWorkers.Clear();

            var availableWorkers = UnitsManager.Instance.UnemployedUnits;
            foreach (var availableWorker in availableWorkers)
            {
                var selectWorker = Instantiate(_selectWorkerDisplayPrefab, _assignWorkerRoot);
                selectWorker.Init(availableWorker, AddWorker);
                _displayedSelectWorkers.Add(selectWorker);
            }
        }

        private void AddWorker(UnitState unitState)
        {
            _zone.AddOccupant(unitState);
            unitState.AssignedWorkRoom = _zone;
            Refresh();
            CloseAssignWorkerPressed();
        }

        private void RemoveWorker(UnitState unitState)
        {
            _zone.RemoveOccupant(unitState);
            unitState.AssignedWorkRoom = null;
            Refresh();
        }
    }
}
