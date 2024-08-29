using System.Collections.Generic;
using Items;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class GenericDetails : MonoBehaviour
    {
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private Transform _commandsParent;
        [SerializeField] private CommandBtn _commandBtnPrefab;
        [SerializeField] private GameObject _durabilityFillHandle;
        [SerializeField] private Image _durabilityFill;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private ResourceDetails _resourceDetails;
        [SerializeField] private StructureDetails _structureDetails;
        [SerializeField] private FurnitureDetails _furnitureDetails;
        [SerializeField] private ItemDetails _itemDetails;
        [SerializeField] private GameObject _controlsSeperator;
        [SerializeField] private GameObject _storageSettingsBtn;
        [SerializeField] private GameObject _headerSeperator;
        
        public TextMeshProUGUI ItemName;
        
        private PlayerInteractable _playerInteractable;
        private List<CommandBtn> _displayedCmds = new List<CommandBtn>();
        
        public void Show(PlayerInteractable playerInteractable)
        {
            _playerInteractable = playerInteractable;
            _panelHandle.SetActive(true);
            _commandBtnPrefab.gameObject.SetActive(false);
            _controlsSeperator.SetActive(false);
            _storageSettingsBtn.SetActive(false);
            
            GameEvents.RefreshSelection += GameEvent_RefreshSelection;
            _playerInteractable.OnChanged += GameEvent_RefreshSelection;
            
            Refresh();
        }

        private void RefreshCommands()
        {
            foreach (var displayedCmd in _displayedCmds)
            {
                Destroy(displayedCmd.gameObject);
            }
            _displayedCmds.Clear();
            
            var commands = _playerInteractable.GetCommands();
            foreach (var command in commands)
            {
                bool isActive = _playerInteractable.PendingCommand == command;
                
                var cmdBtn = Instantiate(_commandBtnPrefab, _commandsParent);
                cmdBtn.transform.SetSiblingIndex(_controlsSeperator.transform.GetSiblingIndex());
                cmdBtn.Init(command, isActive, OnCommandPressed);
                cmdBtn.gameObject.SetActive(true);
                _displayedCmds.Add(cmdBtn);
            }
        }

        public void ShowControlsSeperator(bool shouldShow)
        {
            _controlsSeperator.SetActive(shouldShow);
        }

        public void ShowHeaderSeperator(bool shouldShow)
        {
            _headerSeperator.SetActive(shouldShow);
        }

        public void HideDurabilityFill()
        {
            _durabilityFillHandle.gameObject.SetActive(false);
        }

        public void SetDurabilityFill(float currentAmount, float maxAmount)
        {
            _durabilityFillHandle.SetActive(true);
            float percent = currentAmount / maxAmount;
            _durabilityFill.fillAmount = percent;
            _durabilityText.text = $"HP: {currentAmount} / {maxAmount}";
        }

        private void GameEvent_RefreshSelection()
        {
            if (_playerInteractable == null)
            {
                Hide();
            }
            else
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            RefreshCommands();
            ShowHeaderSeperator(true);
            
            if (_playerInteractable is BasicResource resource)
            {
                _resourceDetails.Show(resource, this);
                _structureDetails.Hide();
                _furnitureDetails.Hide();
                _itemDetails.Hide();
            }
            else if (_playerInteractable is Construction structure)
            {
                _structureDetails.Show(structure, this);
                _resourceDetails.Hide();
                _furnitureDetails.Hide();
                _itemDetails.Hide();
            }
            else if (_playerInteractable is Furniture furniture)
            {
                _furnitureDetails.Show(furniture, this);
                _resourceDetails.Hide();
                _structureDetails.Hide();
                _itemDetails.Hide();
            }
            else if (_playerInteractable is Item item)
            {
                _itemDetails.Show(item, this);
                _resourceDetails.Hide();
                _structureDetails.Hide();
                _furnitureDetails.Hide();
            }
            else
            {
                _itemDetails.Hide();
                _resourceDetails.Hide();
                _structureDetails.Hide();
                _furnitureDetails.Hide();
            }
        }

        private void OnCommandPressed(Command command)
        {
            var pendingCmd = _playerInteractable.PendingCommand;
            if (pendingCmd != null)
            {
                _playerInteractable.CancelPendingTask();
            }
            
            if (pendingCmd != command)
            {
                _playerInteractable.AssignCommand(command);
            }
            
            RefreshCommands();
        }
        
        public void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
        
        public void Hide()
        {
            if (_playerInteractable != null)
            {
                _playerInteractable.OnChanged -= GameEvent_RefreshSelection;
            }
            
            GameEvents.RefreshSelection -= GameEvent_RefreshSelection;
            _panelHandle.SetActive(false);
        }
    }
}
