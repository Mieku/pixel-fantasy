using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class CommandOptionBtn : OptionBtn
    {
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBtnSpr;
        [SerializeField] private Sprite _activeBtnSpr;
        
        private Command _command;
        
        public void Init(Command command, CategoryBtn categoryBtn)
        {
            _ownerCategoryBtn = categoryBtn;
            _command = command;
            _icon.sprite = _command.Icon;
        }

        protected override void ToggledOn()
        {
            // No Details are needed
            _btnBG.sprite = _activeBtnSpr;
        }

        protected override void ToggledOff()
        {
            // No Details are needed
            _btnBG.sprite = _defaultBtnSpr;
        }

        protected override void TriggerOptionEffect()
        {
            SelectionManager.Instance.BeginCommandSelectionBox(_command, SelectionCompleted);
        }

        private void SelectionCompleted()
        {
            Cancel();
        }

        public override void Cancel()
        {
            if (_isOpen)
            {
                SelectionManager.Instance.DeactivateSelectionBox();
            }
            
            base.Cancel();
        }
    }
}
