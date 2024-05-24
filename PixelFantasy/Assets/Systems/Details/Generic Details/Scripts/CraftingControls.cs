using System.Collections.Generic;
using Data.Item;
using Items;
using Systems.Crafting.Scripts;
using Systems.Details.Building_Details.Scripts;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class CraftingControls : MonoBehaviour
    {
        [SerializeField] private CraftingOrderOption _craftingOrderOptionPrefab;
        [SerializeField] private Transform _craftingOptionsParent;
        [SerializeField] private CraftingQueueEntry _craftingQueueEntryPrefab;
        [SerializeField] private Transform _craftingQueueLayout;

        [SerializeField] private CraftingQueueEntry _currentOrderEntry;
        
        private CraftingTableData _data;
        private List<CraftingOrderOption> _displayedOption = new List<CraftingOrderOption>();
        private List<CraftingQueueEntry> _displayedCraftingQueue = new List<CraftingQueueEntry>();

        public void Show(CraftingTable craftingTable)
        {
            _data = craftingTable.RuntimeTableData;
            gameObject.SetActive(true);
            _craftingOrderOptionPrefab.gameObject.SetActive(false);
            _craftingQueueEntryPrefab.gameObject.SetActive(false);
            _currentOrderEntry.gameObject.SetActive(false);
            
            DisplayCraftingOptions();
            DisplayCraftingQueue();
        }

        public void Hide()
        {
            _data = null;
            gameObject.SetActive(false);
        }

        private void DisplayCraftingOptions()
        {
            foreach (var option in _displayedOption)
            {
                Destroy(option.gameObject);
            }
            _displayedOption.Clear();

            var craftables = _data.CraftingTableSettings.GetCraftableItems;
            foreach (var craftable in craftables)
            {
                var option = Instantiate(_craftingOrderOptionPrefab, _craftingOptionsParent);
                option.Init(craftable, OnOptionSelected);
                option.gameObject.SetActive(true);
                _displayedOption.Add(option);
            }
        }

        private void DisplayCraftingQueue()
        {
            foreach (var entry in _displayedCraftingQueue)
            {
                Destroy(entry.gameObject);
            }
            _displayedCraftingQueue.Clear();

            // Current
            var curOrder = _data.CurrentOrder;
            if (curOrder.State != CraftingOrder.EOrderState.None)
            {
                _currentOrderEntry.gameObject.SetActive(true);
                _currentOrderEntry.Init(curOrder, null, false, false, _data.LinkedFurniture.OnChanged);
            }
            else
            {
                _currentOrderEntry.gameObject.SetActive(false);
            }
            
            // Local
            var queue = _data.LocalCraftingQueue.Orders;
            foreach (var order in queue)
            {
                var entry = Instantiate(_craftingQueueEntryPrefab, _craftingQueueLayout);
                entry.gameObject.SetActive(true);
                entry.Init(order, _data.LocalCraftingQueue, !_data.LocalCraftingQueue.IsFirstInQueue(order), !_data.LocalCraftingQueue.IsLastInQueue(order), _data.LinkedFurniture.OnChanged);
                _displayedCraftingQueue.Add(entry);
            }
        }

        private void OnOptionSelected(ItemSettings item)
        {
            var craftedItem = item as CraftedItemSettings;
            if (craftedItem != null)
            {
                CraftingOrder optionOrder = new CraftingOrder(craftedItem, _data.LinkedFurniture, CraftingOrder.EOrderType.Item);
                _data.SubmitOrder(optionOrder);
            }

            var meal = item as MealSettings;
            if (meal != null)
            {
                CraftingOrder optionOrder = new CraftingOrder(meal, _data.LinkedFurniture);
                _data.SubmitOrder(optionOrder);
            }
            
            DisplayCraftingQueue();
        }
    }
}
