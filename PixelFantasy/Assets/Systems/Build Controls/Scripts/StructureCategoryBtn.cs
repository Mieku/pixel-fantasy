using Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class StructureCategoryBtn : CategoryBtn
    {
        [SerializeField] private Image _btnImg;
        [SerializeField] private Sprite _defaultBtn;
        [SerializeField] private Sprite _highlightedBtn;
        
        protected override void ButtonActivated()
        {
            _isActive = true;
            HighlightBtn(true);
            HUDController.Instance.ShowBuildStructureDetails();
        }

        protected override void ButtonDeactivated()
        {
            _isActive = false;
            HighlightBtn(false);
            HUDController.Instance.HideDetails();
        }
        
        public void HighlightBtn(bool isHighlighted)
        {
            if (isHighlighted)
            {
                _btnImg.sprite = _highlightedBtn;
            }
            else
            {
                _btnImg.sprite = _defaultBtn;
            }
        }
    }
}
