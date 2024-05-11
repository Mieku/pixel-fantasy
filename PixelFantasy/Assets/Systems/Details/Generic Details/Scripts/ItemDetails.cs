using Items;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class ItemDetails : MonoBehaviour
    {
        private GenericDetails _parentDetails;
        private Item _item;
        private bool _isActive;
        
        public void Show(Item item, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _parentDetails = parentDetails;
            _item = item;
            _isActive = true;
            
            _parentDetails.ItemName.color = _item.RuntimeData.GetQualityColour();
            _parentDetails.ItemName.text = $"{_item.DisplayName} ({_item.RuntimeData.Quality.GetDescription()})";
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_item.RuntimeData.Durability, _item.RuntimeData.Settings.MaxDurability);
        }
    }
}
