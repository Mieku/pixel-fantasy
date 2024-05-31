using System.Collections.Generic;
using Interfaces;
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
        [SerializeField] private Image _durabilityFill;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private ResourceDetails _resourceDetails;
        [SerializeField] private StructureDetails _structureDetails;
        [SerializeField] private FurnitureDetails _furnitureDetails;
        [SerializeField] private ItemDetails _itemDetails;
        [SerializeField] private GameObject _controlsSeperator;
        [SerializeField] private GameObject _storageSettingsBtn;
        
        public TextMeshProUGUI ItemName;
        
        private EDetailsState _detailsState;
        private IClickableObject _clickableObject;
        private List<CommandBtn> _displayedCmds = new List<CommandBtn>();
        
        
        public void Show(IClickableObject clickableObject)
        {
            _clickableObject = clickableObject;
            _panelHandle.SetActive(true);
            _commandBtnPrefab.gameObject.SetActive(false);
            _controlsSeperator.SetActive(false);
            _storageSettingsBtn.SetActive(false);
            
            GameEvents.RefreshSelection += GameEvent_RefreshSelection;
            _clickableObject.OnChanged += GameEvent_RefreshSelection;
            
            Refresh();
        }

        private void RefreshCommands()
        {
            foreach (var displayedCmd in _displayedCmds)
            {
                Destroy(displayedCmd.gameObject);
            }
            _displayedCmds.Clear();
            
            var commands = _clickableObject.GetCommands();
            foreach (var command in commands)
            {
                bool isActive = _clickableObject.GetPlayerInteractable().PendingCommand == command;
                
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

        public void SetDurabilityFill(float currentAmount, float maxAmount)
        {
            float percent = currentAmount / maxAmount;
            _durabilityFill.fillAmount = percent;
            _durabilityText.text = $"HP: {currentAmount} / {maxAmount}";
        }

        private void GameEvent_RefreshSelection()
        {
            if (_clickableObject == null)
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

            var resource = _clickableObject as BasicResource;
            if (resource != null)
            {
                _resourceDetails.Show(resource, this);
                
                _structureDetails.Hide();
                _furnitureDetails.Hide();
                _itemDetails.Hide();
                
                return;
            }

            var structure = _clickableObject as Construction;
            if (structure != null)
            {
                _structureDetails.Show(structure, this);
                
                _resourceDetails.Hide();
                _furnitureDetails.Hide();
                _itemDetails.Hide();
                
                return;
            }

            var furniture = _clickableObject as Furniture;
            if (furniture != null)
            {
                _furnitureDetails.Show(furniture, this);
                
                _resourceDetails.Hide();
                _structureDetails.Hide();
                _itemDetails.Hide();
                
                return;
            }

            var item = _clickableObject as Item;
            if (item != null)
            {
                _itemDetails.Show(item, this);
                
                _resourceDetails.Hide();
                _structureDetails.Hide();
                _furnitureDetails.Hide();
                
                return;
            }
        }

        private void OnCommandPressed(Command command)
        {
            var pendingCmd = _clickableObject.GetPlayerInteractable().PendingCommand;
            if (pendingCmd != null)
            {
                _clickableObject.GetPlayerInteractable().CancelCommand(pendingCmd);
            }
            
            if (pendingCmd != command)
            {
                _clickableObject.AssignCommand(command);
            }
            
            RefreshCommands();
        }
        
        public void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
        
        public void Hide()
        {
            if (_clickableObject != null)
            {
                _clickableObject.OnChanged -= GameEvent_RefreshSelection;
            }
            
            GameEvents.RefreshSelection -= GameEvent_RefreshSelection;
            _panelHandle.SetActive(false);
        }

        private enum EDetailsState
        {
            
        }
    }
}
