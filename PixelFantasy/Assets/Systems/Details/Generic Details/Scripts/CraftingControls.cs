using System.Collections.Generic;
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
        
        private CraftingTableData _data;
        private List<CraftingOrderOption> _displayedOption = new List<CraftingOrderOption>();
        private List<CraftingQueueEntry> _displayedCraftingQueue = new List<CraftingQueueEntry>();

        public void Show(CraftingTable craftingTable)
        {
            _data = craftingTable.RuntimeTableData;
            gameObject.SetActive(true);
            _craftingOrderOptionPrefab.gameObject.SetActive(false);
            _craftingQueueEntryPrefab.gameObject.SetActive(false);
            
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
            
            // Local
            var queue = _data.LocalCraftingQueue.Orders;
            foreach (var order in queue)
            {
                var entry = Instantiate(_craftingQueueEntryPrefab, _craftingQueueLayout);
                entry.gameObject.SetActive(true);
                entry.Init(order, _data.LocalCraftingQueue, _data.GetLinkedFurniture().OnChanged);
                _displayedCraftingQueue.Add(entry);
            }
        }

        private void OnOptionSelected(ItemSettings item)
        {
            CraftingOrder optionOrder = new CraftingOrder(item.name);
            _data.SubmitOrder(optionOrder);
            
            DisplayCraftingQueue();
        }
    }
}
