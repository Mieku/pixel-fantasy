using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class ItemDetails : MonoBehaviour
    {
        [SerializeField] private DetailsTextEntry _textEntryPrefab;
        
        private GenericDetails _parentDetails;
        private Item _item;
        private bool _isActive;
        private List<DetailsTextEntry> _displayedTextEntries = new List<DetailsTextEntry>();
        
        public void Show(Item item, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _textEntryPrefab.gameObject.SetActive(false);
            _parentDetails = parentDetails;
            _item = item;
            _isActive = true;
            
            _parentDetails.ItemName.color = _item.RuntimeData.GetQualityColour();
            _parentDetails.ItemName.text = $"{_item.DisplayName} ({_item.RuntimeData.Quality.GetDescription()})";
            
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
            
            var detailsTexts = _item.RuntimeData.GetDetailsTexts();
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
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_item.RuntimeData.Durability, _item.RuntimeData.Settings.MaxDurability);
        }
    }
}
