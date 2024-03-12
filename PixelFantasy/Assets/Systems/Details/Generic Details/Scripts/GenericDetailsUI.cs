// using System;
// using System.Collections.Generic;
// using HUD;
// using Interfaces;
// using Items;
// using Managers;
// using TMPro;
// using UnityEngine;
//
// namespace Systems.Details.Generic_Details.Scripts
// {
//     public class GenericDetailsUI : MonoBehaviour
//     {
//         [SerializeField] private TextMeshProUGUI _title;
//         [SerializeField] private Transform _contentLayout;
//         [SerializeField] private GameObject _panelHandle;
//         [SerializeField] private DetailsEntryBarDisplay _entryBarDisplayPrefab;
//         [SerializeField] private DetailsEntryResourcesDisplay _entryResourcesDisplayPrefab;
//         [SerializeField] private DetailsEntryTitledTextDisplay _entryTitledTextDisplayPrefab;
//         [SerializeField] private DetailsEntryTextDisplay _entryTextDisplayPrefab;
//         [SerializeField] private DetailsEntryInventoryDisplay _entryInventoryDisplayPrefab;
//         [SerializeField] private OrderButton _orderButtonPrefab;
//         [SerializeField] private Transform _ordersLayout;
//         
//         private List<GameObject> _displayedEntries = new List<GameObject>();
//         private IClickableObject _clickableObject;
//         private List<OrderButton> _displayedCommands = new List<OrderButton>();
//
//         public void Show(IClickableObject clickableObject)
//         {
//             _panelHandle.SetActive(true);
//             _clickableObject = clickableObject;
//
//             _title.text = _clickableObject.DisplayName;
//
//             DisplayCommands(_clickableObject.GetCommands());
//
//             ApplyResourceEntries(_clickableObject);
//             ApplyItemEntries(_clickableObject);
//             ApplyFurnitureEntries(_clickableObject);
//
//             ApplyFurnitureButtons();
//         }
//
//         private void ApplyResourceEntries(IClickableObject clickableObject)
//         {
//             var resource = clickableObject as Resource;
//             if (resource != null)
//             {
//                 // Health
//                 var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
//                 bar.Init("Health", resource.GetHealthIcon(), resource.GetHealthPercentage);
//                 _displayedEntries.Add(bar.gameObject);
//             }
//
//             var growingResource = clickableObject as GrowingResource;
//             if (growingResource != null)
//             {
//                 // Growth
//                 var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
//                 bar.Init("Growth", growingResource.GetGrowthIcon(), growingResource.GetGrowthPercentage);
//                 _displayedEntries.Add(bar.gameObject);
//
//                 // Fruit
//                 if (growingResource.IsFruiting)
//                 {
//                     var fruitBar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
//                     fruitBar.Init("Fruit", growingResource.GetFruitIcon(), growingResource.GetFruitingPercentage);
//                     _displayedEntries.Add(fruitBar.gameObject);
//                 }
//             }
//             
//             if (resource != null) 
//             {
//                 // Yield
//                 var harvestResources = resource.GetHarvestableItems().GetDropAverages();
//                 if (harvestResources.Count > 0)
//                 {
//                     var resourceEntry = Instantiate(_entryResourcesDisplayPrefab, _contentLayout);
//                     resourceEntry.Init(harvestResources);
//                     _displayedEntries.Add(resourceEntry.gameObject);
//                 }
//             }
//
//             if (growingResource != null)
//             {
//                 if (growingResource.IsFruiting && growingResource.HasFruitAvailable)
//                 {
//                     var textEntry = Instantiate(_entryTextDisplayPrefab, _contentLayout);
//                     textEntry.Init("Fruit Can Be Harvested!");
//                     _displayedEntries.Add(textEntry.gameObject);
//                 }
//             }
//         }
//
//         private void ApplyItemEntries(IClickableObject clickableObject)
//         {
//             var item = clickableObject as Item;
//             if (item != null)
//             {
//                 // Durability
//                 var durabilityIcon = Librarian.Instance.GetSprite("Health");
//                 var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
//                 bar.Init("Durability", durabilityIcon, item.State.DurabilityPercentage);
//                 _displayedEntries.Add(bar.gameObject);
//
//                 if (item.State.WasCrafted)
//                 {
//                     var craftersName = item.State.GetCrafter().FullName;
//                     var titledEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
//                     titledEntry.Init("Crafted By", craftersName);
//                     _displayedEntries.Add(bar.gameObject);
//                 }
//             }
//         }
//
//         private void ApplyFurnitureEntries(IClickableObject clickableObject)
//         {
//             var furniture = clickableObject as Furniture;
//             if(furniture == null) return;
//             
//             // Durability
//             var durabilityIcon = Librarian.Instance.GetSprite("Health");
//             var bar = Instantiate(_entryBarDisplayPrefab, _contentLayout);
//             
//             bar.Init("Durability", durabilityIcon, furniture.Data.DurabilityPercentage);
//             _displayedEntries.Add(bar.gameObject);
//             
//             // Category
//             //var category = furniture.FurnitureItemData.Catergory.GetDescription();
//             // var categoryEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
//             // categoryEntry.Init("Type", category);
//             // _displayedEntries.Add(categoryEntry.gameObject);
//             
//             // Description
//             var description = furniture.Settings.ItemDescription;
//             if (!string.IsNullOrEmpty(description))
//             {
//                 var descriptionEntry = Instantiate(_entryTextDisplayPrefab, _contentLayout);
//                 descriptionEntry.Init(description);
//                 _displayedEntries.Add(descriptionEntry.gameObject);
//             }
//
//             // TODO: Storage
//             
//             // Assigned to
//             // if (furniture.SingleOwner)
//             // {
//             //     var assignedEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
//             //     if (furniture.AssignedKinling == null)
//             //     {
//             //         
//             //         assignedEntry.Init("Assigned To", "Unassigned");
//             //         
//             //     }
//             //     else
//             //     {
//             //         assignedEntry.Init("Assigned To", furniture.AssignedKinling.GetUnitState().FullName);
//             //     }
//             //     _displayedEntries.Add(assignedEntry.gameObject);
//             // }
//             
//             // Order Details
//             // if (furniture.FurnitureState == Furniture.EFurnitureState.Craftable)
//             // {
//             //     // Crafter Job
//             //     string craftMsg = "";
//             //     
//             //     foreach (var option in furniture.FurnitureItemData.RequiredCraftingTableOptions)
//             //     {
//             //         if (!string.IsNullOrEmpty(craftMsg))
//             //             craftMsg += ", ";
//             //
//             //         craftMsg += $"{option.ItemName} ";
//             //     }
//             //
//             //     if (!string.IsNullOrEmpty(craftMsg))
//             //     {
//             //         if (!furniture.CanBeCrafted())
//             //         {
//             //             craftMsg = $"<color=red>{craftMsg}</color>";
//             //         }
//             //         
//             //         var requiresEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
//             //         requiresEntry.Init("Requires", craftMsg);
//             //         _displayedEntries.Add(requiresEntry.gameObject);
//             //     }
//             //     
//             //     var craftCosts = furniture.FurnitureItemData.GetResourceCosts();
//             //     var resourceEntry = Instantiate(_entryResourcesDisplayPrefab, _contentLayout);
//             //     resourceEntry.Init(craftCosts, "Materials", false);
//             //     _displayedEntries.Add(resourceEntry.gameObject);
//             // }
//             
//             // Storage
//             var storage = furniture as Storage;
//             if (storage != null)
//             {
//                 if (storage.Data.State == FurnitureData.EFurnitureState.Built)
//                 {
//                     // Inventory
//                     var inventoryEntry = Instantiate(_entryInventoryDisplayPrefab, _contentLayout);
//                     inventoryEntry.Init(storage);
//                     _displayedEntries.Add(inventoryEntry.gameObject);
//                 }
//             }
//             
//             // Crafted by
//             if (furniture.WasCrafted)
//             {
//                 var craftersName = furniture.GetCrafter().FullName;
//                 var craftersEntry = Instantiate(_entryTitledTextDisplayPrefab, _contentLayout);
//                 craftersEntry.Init("Crafted By", craftersName);
//                 _displayedEntries.Add(craftersEntry.gameObject);
//             }
//         }
//
//         private void ApplyFurnitureButtons()
//         {
//             var furniture = _clickableObject as Furniture;
//             if (furniture == null) return;
//
//             // if (furniture.FurnitureState == Furniture.EFurnitureState.Craftable)
//             // {
//             //     Sprite orderIcon = Librarian.Instance.GetSprite("Order Icon");
//             //     void OnPressed()
//             //     {
//             //         furniture.Order();
//             //     }
//             //     CreateOrderButton(orderIcon, OnPressed, false, "Order");
//             // }
//             
//             if (furniture.Data.State != FurnitureData.EFurnitureState.InProduction)
//             {
//                 Sprite orderIcon = Librarian.Instance.GetSprite("Move Icon");
//                 void OnPressed()
//                 {
//                     furniture.Trigger_Move();
//                 }
//                 CreateOrderButton(orderIcon, OnPressed, false, "Move");
//             }
//         }
//         
//         public void DisplayCommands(List<Command> commands)
//         {
//             ClearCommands();
//             foreach (var command in commands)
//             {
//                 var playerInteractable = _clickableObject.GetPlayerInteractable();
//                 bool isActive = playerInteractable.IsPending(command);
//                 CreateCommand(command, playerInteractable, isActive);
//             }
//         }
//         
//         public void ClearCommands()
//         {
//             foreach (var order in _displayedCommands)
//             {
//                 Destroy(order.gameObject);
//             }
//             _displayedCommands.Clear();
//         }
//
//         private void CreateCommand(Command command, PlayerInteractable requestor, bool isActive)
//         {
//             Sprite icon = command.Icon;
//
//             void OnPressed()
//             {
//                 if (isActive)
//                 {
//                     requestor.CancelCommand(command);
//                 }
//                 else
//                 {
//                     requestor.CreateTask(command);
//                 }
//                 GameEvents.Trigger_RefreshSelection();
//             }
//             
//             CreateOrderButton(icon, OnPressed, isActive, command.Name);
//         }
//
//         public void CreateOrderButton(Sprite icon, Action onPressed, bool isActive, string buttonName)
//         {
//             var orderBtn = Instantiate(_orderButtonPrefab, _ordersLayout);
//             orderBtn.Init(icon, onPressed, isActive, buttonName);
//             _displayedCommands.Add(orderBtn);
//         }
//         
//         void Update()
//         {
//             foreach (var entry in _displayedEntries)
//             {
//                 var bar = entry.GetComponent<DetailsEntryBarDisplay>();
//                 if (bar != null)
//                 {
//                     bar.RefreshFill();
//                 }
//             }
//         }
//
//         public void Hide()
//         {
//             _panelHandle.SetActive(false);
//             ClearDisplayedEntries();
//         }
//
//         private void ClearDisplayedEntries()
//         {
//             foreach (var entry in _displayedEntries)
//             {
//                 Destroy(entry);
//             }
//             _displayedEntries.Clear();
//         }
//     }
// }
