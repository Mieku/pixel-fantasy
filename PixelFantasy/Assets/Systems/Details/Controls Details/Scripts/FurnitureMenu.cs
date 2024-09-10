using ScriptableObjects;
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
        [SerializeField] private ControlsBtn _lightingBtn;

        [SerializeField] private BuildFurnitureDetailsUI _buildFurnitureDetails;
        
        public override void Show()
        {
            base.Show();
            
            _buildFurnitureDetails.Hide();
            _subMenuSeperator.SetActive(false);

            _storageBtn.OnPressed += StoragePressed;
            _decorationsBtn.OnPressed += DecorationsPressed;
            _lifestyleBtn.OnPressed += LifestylePressed;
            _craftingBtn.OnPressed += CraftingPressed;
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
            _lightingBtn.OnPressed -= LightingPressed;
        }

        private void StoragePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            
            var options =
                GameSettings.Instance.PlayerBuildCategories.GetAllSettingsByCategory<FurnitureSettings>(ESettingsCategory.Furniture_Storage);
            _buildFurnitureDetails.Show(options);
        }
        
        private void DecorationsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            
            var options =
                GameSettings.Instance.PlayerBuildCategories.GetAllSettingsByCategory<FurnitureSettings>(ESettingsCategory.Furniture_Decorations);
            _buildFurnitureDetails.Show(options);
        }
        
        private void LifestylePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            
            var options =
                GameSettings.Instance.PlayerBuildCategories.GetAllSettingsByCategory<FurnitureSettings>(ESettingsCategory.Furniture_Lifestyle);
            _buildFurnitureDetails.Show(options);
        }
        
        private void CraftingPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            
            var options =
                GameSettings.Instance.PlayerBuildCategories.GetAllSettingsByCategory<FurnitureSettings>(ESettingsCategory.Furniture_Crafting);
            _buildFurnitureDetails.Show(options);
        }
        
        private void LightingPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            
            var options =
                GameSettings.Instance.PlayerBuildCategories.GetAllSettingsByCategory<FurnitureSettings>(ESettingsCategory.Furniture_Lighting);
            _buildFurnitureDetails.Show(options);
        }
    }
}
