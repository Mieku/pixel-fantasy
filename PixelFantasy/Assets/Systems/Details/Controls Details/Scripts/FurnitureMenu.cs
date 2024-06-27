using System.Collections.Generic;
using System.Linq;
using Databrain;
using Databrain.Attributes;
using Systems.Details.Build_Details.Scripts;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class FurnitureMenu : ControlsMenu
    {
        [SerializeField] private GameObject _subMenuSeperator;
        [SerializeField] private ControlsBtn _storageBtn;
        [SerializeField] private ControlsBtn _decorationsBtn;
        [SerializeField] private ControlsBtn _lifestyleBtn;
        [SerializeField] private ControlsBtn _craftingBtn;
        [SerializeField] private ControlsBtn _productionBtn;
        [SerializeField] private ControlsBtn _lightingBtn;

        [SerializeField] private BuildFurnitureDetailsUI _buildFurnitureDetails;
        
        public DataLibrary DataLibrary;
        
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<StorageSettings> _storageOptions;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureSettings> _decorationOptions;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<CraftingTableSettings> _productionOptions;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<CraftingTableSettings> _craftingOptions;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureSettings> _lightingOptions;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureSettings> _lifestyleOptions;
        
        public override void Show()
        {
            base.Show();
            
            _buildFurnitureDetails.Hide();
            _subMenuSeperator.SetActive(false);

            _storageBtn.OnPressed += StoragePressed;
            _decorationsBtn.OnPressed += DecorationsPressed;
            _lifestyleBtn.OnPressed += LifestylePressed;
            _craftingBtn.OnPressed += CraftingPressed;
            _productionBtn.OnPressed += ProductionPressed;
            _lightingBtn.OnPressed += LightingPressed;
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            
            _buildFurnitureDetails.Hide();
            _subMenuSeperator.SetActive(false);
            
            _storageBtn.OnPressed -= StoragePressed;
            _decorationsBtn.OnPressed -= DecorationsPressed;
            _lifestyleBtn.OnPressed -= LifestylePressed;
            _craftingBtn.OnPressed -= CraftingPressed;
            _productionBtn.OnPressed -= ProductionPressed;
            _lightingBtn.OnPressed -= LightingPressed;
        }

        private void StoragePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_storageOptions.Cast<FurnitureSettings>().ToList());
        }
        
        private void DecorationsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_decorationOptions);
        }
        
        private void LifestylePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_lifestyleOptions);
        }
        
        private void CraftingPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_craftingOptions.Cast<FurnitureSettings>().ToList());
        }
        
        private void ProductionPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_productionOptions.Cast<FurnitureSettings>().ToList());
        }
        
        private void LightingPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildFurnitureDetails.Show(_lightingOptions);
        }
    }
}
