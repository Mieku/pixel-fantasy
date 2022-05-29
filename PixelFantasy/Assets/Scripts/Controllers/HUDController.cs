using System;
using System.Collections;
using System.Collections.Generic;
using Gods;
using HUD;
using UnityEngine;

namespace Controllers
{
    public class HUDController : God<HUDController>
    {
        [SerializeField] private SelectedItemInfoPanel _selectedItemInfoPanel;
        
        public void ShowItemDetails(SelectionData selectionData)
        {
            _selectedItemInfoPanel.ShowItemDetails(selectionData);
        }

        public void HideItemDetails()
        {
            if (_selectedItemInfoPanel != null)
            {
                _selectedItemInfoPanel.HideItemDetails();
            }
        }
    }
    
    
}