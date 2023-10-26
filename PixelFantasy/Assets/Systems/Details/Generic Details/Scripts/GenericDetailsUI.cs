using System;
using System.Collections.Generic;
using HUD;
using Interfaces;
using Items;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Details.Generic_Details.Scripts
{
    public class GenericDetailsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Transform _contentLayout;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private DetailsEntryBarDisplay _entryBarDisplayPrefab;
        [SerializeField] private DetailsEntryResourcesDisplay _entryResourcesDisplayPrefab;
        [SerializeField] private DetailsEntryTitledTextDisplay _entryTitledTextDisplayPrefab;
        [SerializeField] private DetailsEntryTextDisplay _entryTextDisplayPrefab;
        [SerializeField] private OrderButton _orderButtonPrefab;
        [SerializeField] private Transform _ordersLayout;
        
        private List<GameObject> _displayedEntries = new List<GameObject>();
        private IClickableObject _clickableObject;
        private List<OrderButton> _displayedCommands = new List<OrderButton>();

        public void Show(IClickableObject clickableObject)
        {
            _panelHandle.SetActive(true);
            _clickableObject = clickableObject;

            _title.text = _clickableObject.DisplayName;

            DisplayCommands(_clickableObject.GetCommands());

            ApplyResourceEntries(_clickableObject);
            ApplyItemEntries(_clickableObject);
        }

        private void ApplyResourceEntries(IClickableObject clickableObject)
        {
            var resource = clickableObject as Resource;
            if (resource != null)
            {
                // Health
                var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
                bar.Init("Health", resource.GetHealthIcon(), resource.GetHealthPercentage);
                _displayedEntries.Add(bar.gameObject);
            }

            var growingResource = clickableObject as GrowingResource;
            if (growingResource != null)
            {
                // Growth
                var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
                bar.Init("Growth", growingResource.GetGrowthIcon(), growingResource.GetGrowthPercentage);
                _displayedEntries.Add(bar.gameObject);

                // Fruit
                if (growingResource.IsFruiting)
                {
                    var fruitBar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
                    fruitBar.Init("Fruit", growingResource.GetFruitIcon(), growingResource.GetFruitingPercentage);
                    _displayedEntries.Add(fruitBar.gameObject);
                }
            }
            
            if (resource != null) 
            {
                // Yield
                var harvestResources = resource.GetHarvestableItems().GetDropAverages();
                if (harvestResources.Count > 0)
                {
                    var resourceEntry = Instantiate(_entryResourcesDisplayPrefab, _contentLayout);
                    resourceEntry.Init(harvestResources);
                    _displayedEntries.Add(resourceEntry.gameObject);
                }
            }
        }

        private void ApplyItemEntries(IClickableObject clickableObject)
        {
            var item = clickableObject as Item;
            if (item != null)
            {
                // Durability
                var durabilityIcon = Librarian.Instance.GetSprite("Health");
                var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
                bar.Init("Durability", durabilityIcon, item.State.DurabilityPercentage);
                _displayedEntries.Add(bar.gameObject);

                if (item.State.WasCrafted)
                {
                    var craftersName = item.State.GetCrafter().GetUnitState().FullName;
                    var titledEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
                    titledEntry.Init("Crafted By", craftersName);
                    _displayedEntries.Add(bar.gameObject);
                }
            }
        }
        
        public void DisplayCommands(List<Command> commands)
        {
            ClearCommands();
            foreach (var command in commands)
            {
                var playerInteractable = _clickableObject.GetPlayerInteractable();
                bool isActive = playerInteractable.IsPending(command);
                CreateCommand(command, playerInteractable, isActive);
            }
        }
        
        public void ClearCommands()
        {
            foreach (var order in _displayedCommands)
            {
                Destroy(order.gameObject);
            }
            _displayedCommands.Clear();
        }

        private void CreateCommand(Command command, PlayerInteractable requestor, bool isActive)
        {
            Sprite icon = command.Icon;

            void OnPressed()
            {
                if (isActive)
                {
                    requestor.CancelCommand(command);
                }
                else
                {
                    requestor.CreateTask(command);
                }
                GameEvents.Trigger_RefreshSelection();
            }
            
            CreateOrderButton(icon, OnPressed, isActive, command.Name);
        }

        public void CreateOrderButton(Sprite icon, Action onPressed, bool isActive, string buttonName)
        {
            var orderBtn = Instantiate(_orderButtonPrefab, _ordersLayout);
            orderBtn.Init(icon, onPressed, isActive, buttonName);
            _displayedCommands.Add(orderBtn);
        }
        
        void Update()
        {
            foreach (var entry in _displayedEntries)
            {
                var bar = entry.GetComponent<DetailsEntryBarDisplay>();
                if (bar != null)
                {
                    bar.RefreshFill();
                }
            }
        }

        public void Hide()
        {
            _panelHandle.SetActive(false);
            ClearDisplayedEntries();
        }

        private void ClearDisplayedEntries()
        {
            foreach (var entry in _displayedEntries)
            {
                Destroy(entry);
            }
            _displayedEntries.Clear();
        }
    }
}
