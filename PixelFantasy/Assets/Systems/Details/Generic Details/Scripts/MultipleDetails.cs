using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class MultipleDetails : MonoBehaviour
    {
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private Transform _groupingsLayout;
        [SerializeField] private SelectedGroupDisplay _groupDisplayPrefab;
        [SerializeField] private Transform _commandsParent;
        [SerializeField] private CommandBtn _commandBtnPrefab;

        private List<PlayerInteractable> _selectedPIs = new List<PlayerInteractable>();
        private readonly List<SelectedGroupDisplay> _displayedGroups = new List<SelectedGroupDisplay>();
        private readonly List<CommandBtn> _displayedCmds = new List<CommandBtn>();

        public void Show(List<PlayerInteractable> playerInteractables)
        {
            _panelHandle.SetActive(true);
            _commandBtnPrefab.gameObject.SetActive(false);
            
            ClearDisplayedGroups();
            
            _groupDisplayPrefab.gameObject.SetActive(false);

            _selectedPIs = playerInteractables;
            int amount = 0;
            foreach (var pi in _selectedPIs)
            {
                amount += pi.GetStackSize();
            }
            
            _headerText.text = $"Multiple Selected (x{amount})";
            SortAndDisplayGroups();
            RefreshCommands();
        }
        
        private void RefreshCommands()
        {
            foreach (var displayedCmd in _displayedCmds)
            {
                Destroy(displayedCmd.gameObject);
            }
            _displayedCmds.Clear();

            List<Command> allCmds = new List<Command>();
            foreach (var pi in _selectedPIs)
            {
                foreach (var command in pi.GetCommands())
                {
                    if (!allCmds.Contains(command))
                    {
                        allCmds.Add(command);
                    }
                }
            }
            
            foreach (var command in allCmds)
            {
                bool isActive = _selectedPIs.Any(pi => pi.PendingCommand == command);
                
                var cmdBtn = Instantiate(_commandBtnPrefab, _commandsParent);
                cmdBtn.Init(command, isActive, OnCommandPressed);
                cmdBtn.gameObject.SetActive(true);
                _displayedCmds.Add(cmdBtn);
            }
        }
        
        private void OnCommandPressed(Command command)
        {
            bool allActive = true;
            List<PlayerInteractable> applicablePIs = new List<PlayerInteractable>();
            foreach (var pi in _selectedPIs)
            {
                if (pi.GetCommands().Contains(command))
                {
                    if (pi.PendingCommand != command)
                    {
                        allActive = false;
                    }

                    applicablePIs.Add(pi);
                }
            }
            
            if (allActive)
            {
                // Deactivate all
                foreach (var pi in applicablePIs)
                {
                    if (pi.PendingCommand == command)
                    {
                        pi.CancelPendingTask();
                    }
                }
            }
            else
            {
                // Activate any that aren't
                foreach (var pi in applicablePIs)
                {
                    if (pi.PendingCommand != command)
                    {
                        pi.CancelPendingTask();
                        pi.AssignCommand(command);
                    }
                }
            }
            
            RefreshCommands();
        }

        public void Hide()
        {
            foreach (var pi in _selectedPIs)
            {
                pi.OnDestroyed -= OnPIDestroyed;
                pi.OnChanged -= GameEvent_RefreshSelection;
            }
            
            ClearDisplayedGroups();
            
            _panelHandle.SetActive(false);
        }

        private void ClearDisplayedGroups()
        {
            foreach (var group in _displayedGroups)
            {
                Destroy(group.gameObject);
            }
            _displayedGroups.Clear();
        }

        private void SortAndDisplayGroups()
        {
            List<List<PlayerInteractable>> groups = new List<List<PlayerInteractable>>();

            // Sorting
            foreach (var pi in _selectedPIs)
            {
                pi.OnDestroyed += OnPIDestroyed;
                pi.OnChanged += GameEvent_RefreshSelection;
                
                if (pi is Kinling) // Kinlings group individually
                {
                    groups.Add(new List<PlayerInteractable>() {pi});
                } 
                else
                {
                    var group = groups.Find(g => g.FirstOrDefault()?.IsSimilar(pi) == true);
                    if (group != null)
                    {
                        group.Add(pi);
                    }
                    else
                    {
                        groups.Add(new List<PlayerInteractable>() {pi});
                    }
                }
            }
            
            // Create the display groups
            foreach (var group in groups)
            {
                if (group is { Count: > 0 })
                {
                    // Kinlings are displayed differently
                    if (group.First() is Kinling kinling)
                    {
                        var kinlingDisplay = Instantiate(_groupDisplayPrefab, _groupingsLayout);
                        kinlingDisplay.gameObject.SetActive(true);
                        kinlingDisplay.Init(group, OnGroupSelected, OnGroupRemoved, kinling.FullName);
                        _displayedGroups.Add(kinlingDisplay);
                    }
                    // Trees are grouped together
                    else if (group.First() is TreeResource tree)
                    {
                        var display = Instantiate(_groupDisplayPrefab, _groupingsLayout);
                        display.gameObject.SetActive(true);
                        display.Init(group, OnGroupSelected, OnGroupRemoved, $"{group.Count} Trees");
                        _displayedGroups.Add(display);
                    }
                    else
                    {
                        var display = Instantiate(_groupDisplayPrefab, _groupingsLayout);
                        display.gameObject.SetActive(true);
                        display.Init(group, OnGroupSelected, OnGroupRemoved);
                        _displayedGroups.Add(display);
                    }
                }
            }
            
            RefreshLayout();
        }

        private void OnGroupSelected(List<PlayerInteractable> group)
        {
            List<PlayerInteractable> unselectPIs = new List<PlayerInteractable>();
            
            foreach (var pi in _selectedPIs)
            {
                if (!group.Contains(pi))
                {
                    unselectPIs.Add(pi);
                }
            }
            
            SelectionManager.Instance.UnSelect(unselectPIs);
        }

        private void OnGroupRemoved(List<PlayerInteractable> group)
        {
            List<PlayerInteractable> unselectPIs = new List<PlayerInteractable>();
            
            foreach (var pi in _selectedPIs)
            {
                if (group.Contains(pi))
                {
                    unselectPIs.Add(pi);
                }
            }
            
            SelectionManager.Instance.UnSelect(unselectPIs);
        }
        
        private void OnPIDestroyed(PlayerInteractable destroyedPI)
        {
            SelectionManager.Instance.UnSelect(new List<PlayerInteractable>() {destroyedPI});
        }
        
        private void GameEvent_RefreshSelection()
        {
            RefreshCommands();
        }
        
        public void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
    }
}
