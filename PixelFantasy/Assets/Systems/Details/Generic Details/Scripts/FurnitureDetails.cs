using Items;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class FurnitureDetails : MonoBehaviour
    {
        [SerializeField] private CraftingControls _craftingControls;
        
        private GenericDetails _parentDetails;
        private Furniture _furniture;
        private bool _isActive;
        
        public void Show(Furniture furniture, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _parentDetails = parentDetails;
            _furniture = furniture;
            _isActive = true;

            _parentDetails.ItemName.color = _furniture.RuntimeData.GetQualityColour();
            _parentDetails.ItemName.text = $"{_furniture.DisplayName} ({_furniture.RuntimeData.Quality.GetDescription()})";

            var craftingTable = _furniture as CraftingTable;
            if (craftingTable != null)
            {
                _craftingControls.Show(craftingTable);
            }
            else
            {
                _craftingControls.Hide();
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_furniture.RuntimeData.Durability, _furniture.RuntimeData.Settings.MaxDurability);
        }
    }
}
