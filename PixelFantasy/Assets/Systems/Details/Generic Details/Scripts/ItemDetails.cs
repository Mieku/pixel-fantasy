using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class ItemDetails : MonoBehaviour
    {
        [SerializeField] private DetailsTextEntry _textEntryPrefab;
        
        private GenericDetails _parentDetails;
        private ItemStack _stack;
        private bool _isActive;
        private List<DetailsTextEntry> _displayedTextEntries = new List<DetailsTextEntry>();
        
        public void Show(ItemStack itemStack, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _textEntryPrefab.gameObject.SetActive(false);
            _parentDetails = parentDetails;
            _stack = itemStack;
            _isActive = true;

            if (itemStack.StackAmount > 1)
            {
                _parentDetails.ItemName.color = Librarian.Instance.GetColour("Common Quality");
                _parentDetails.ItemName.text = $"{_stack.DisplayName} (x{itemStack.StackAmount})";
            }
            else
            {
                var itemData = itemStack.ItemDatas.First();
                _parentDetails.ItemName.color = itemData.GetQualityColour();
                _parentDetails.ItemName.text = $"{_stack.DisplayName} ({itemData.Quality.GetDescription()})";
            }
            
            RefreshTextEntries();
            
            // If there is no details to show in item, hide the header seperator
            if (_displayedTextEntries.Count == 0)
            {
                _parentDetails.ShowHeaderSeperator(false);
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }

        private void RefreshTextEntries()
        {
            foreach (var displayedTextEntry in _displayedTextEntries)
            {
                Destroy(displayedTextEntry.gameObject);
            }
            _displayedTextEntries.Clear();

            if (_stack.StackAmount == 1)
            {
                var itemData = _stack.ItemDatas.First();
                var detailsTexts = itemData.GetDetailsTexts();
                foreach (var detailsText in detailsTexts)
                {
                    var display = Instantiate(_textEntryPrefab, transform);
                    display.gameObject.SetActive(true);
                    if (detailsText.HasHeader)
                    {
                        display.Init(detailsText.Message, detailsText.Header);
                    }
                    else
                    {
                        display.Init(detailsText.Message);
                    }
                    _displayedTextEntries.Add(display);
                }
            }
        }
        
        private void Update()
        {
            if(!_isActive) return;

            if (_stack.StackAmount == 1)
            {
                var itemData = _stack.ItemDatas.First();
                _parentDetails.SetDurabilityFill(itemData.Durability, _stack.Settings.MaxDurability);
            }
            else
            {
                _parentDetails.HideDurabilityFill();
            }
            
            
        }
    }
}
