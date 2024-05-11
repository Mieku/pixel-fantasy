using Items;
using Managers;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StructureDetails : MonoBehaviour
    {
        private GenericDetails _parentDetails;
        private Construction _structure;
        private bool _isActive;
        
        public void Show(Construction structure, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _parentDetails = parentDetails;
            _structure = structure;
            _isActive = true;
            
            _parentDetails.ItemName.color = Librarian.Instance.GetColour("Common Quality");
            _parentDetails.ItemName.text = _structure.DisplayName;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_structure.RuntimeData.Durability, _structure.RuntimeData.MaxDurability);
        }
    }
}
