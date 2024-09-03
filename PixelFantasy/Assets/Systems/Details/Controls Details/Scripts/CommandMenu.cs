using Managers;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class CommandMenu : ControlsMenu
    {
        [SerializeField] private ControlsBtn _cancelCommandBtn;
        [SerializeField] private ControlsBtn _gatherCommandBtn;
        [SerializeField] private ControlsBtn _cutTreeCommandBtn;
        [SerializeField] private ControlsBtn _harvestCommandBtn;
        [SerializeField] private ControlsBtn _mineCommandBtn;
        [SerializeField] private ControlsBtn _deconstructCommandBtn;
        [SerializeField] private ControlsBtn _allowCommandBtn;
        [SerializeField] private ControlsBtn _forbidCommandBtn;
        
        [SerializeField] private Command _cancelCommand;
        [SerializeField] private Command _gatherCommand;
        [SerializeField] private Command _cutTreeCommand;
        [SerializeField] private Command _harvestCommand;
        [SerializeField] private Command _mineCommand;
        [SerializeField] private Command _deconstructCommand;
        [SerializeField] private Command _allowCommand;
        [SerializeField] private Command _forbidCommand;

        public override void Show()
        {
            base.Show();

            _cancelCommandBtn.OnPressed += CancelCmdPressed;
            _gatherCommandBtn.OnPressed += GatherCmdPressed;
            _cutTreeCommandBtn.OnPressed += CutTreeCmdPressed;
            _harvestCommandBtn.OnPressed += HarvestCmdPressed;
            _mineCommandBtn.OnPressed += MineCmdPressed;
            _deconstructCommandBtn.OnPressed += DeconstructCmdPressed;
            _allowCommandBtn.OnPressed += AllowCmdPressed;
            _forbidCommandBtn.OnPressed += ForbidCmdPressed;
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            SelectionManager.Instance.CancelCommandSelectionBox();
            
            _cancelCommandBtn.OnPressed -= CancelCmdPressed;
            _gatherCommandBtn.OnPressed -= GatherCmdPressed;
            _cutTreeCommandBtn.OnPressed -= CutTreeCmdPressed;
            _harvestCommandBtn.OnPressed -= HarvestCmdPressed;
            _mineCommandBtn.OnPressed -= MineCmdPressed;
            _deconstructCommandBtn.OnPressed -= DeconstructCmdPressed;
            _allowCommandBtn.OnPressed -= AllowCmdPressed;
            _forbidCommandBtn.OnPressed -= ForbidCmdPressed;
        }

        #region Button Hooks

        private void CancelCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _cancelCommand, DeselectBtn);
        }
        
        private void GatherCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _gatherCommand, DeselectBtn);
        }
        
        private void CutTreeCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _cutTreeCommand, DeselectBtn);
        }
        
        private void HarvestCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _harvestCommand, DeselectBtn);
        }
        
        private void MineCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _mineCommand, DeselectBtn);
        }
        
        private void DeconstructCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _deconstructCommand, DeselectBtn);
        }
        
        private void AllowCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _allowCommand, DeselectBtn);
        }
        
        private void ForbidCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _forbidCommand, DeselectBtn);
        }

        private void DeselectBtn(ControlsBtn btn)
        {
            _activeBtn = null;
            btn.SetActive(false);
        }

        #endregion

        private void HandleCommandSelection(ControlsBtn btn, Command command, System.Action<ControlsBtn> onCompleted)
        {
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(command, () =>
            {
                // Re-trigger the same command after completion if needed
                onCompleted(btn);
            });
        }
    }
}
