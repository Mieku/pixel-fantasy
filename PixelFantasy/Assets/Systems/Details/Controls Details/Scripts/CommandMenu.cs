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
        
        [SerializeField] private Command _cancelCommand;
        [SerializeField] private Command _gatherCommand;
        [SerializeField] private Command _cutTreeCommand;
        [SerializeField] private Command _harvestCommand;
        [SerializeField] private Command _mineCommand;
        [SerializeField] private Command _deconstructCommand;

        public override void Show()
        {
            base.Show();

            _cancelCommandBtn.OnPressed += CancelCmdPressed;
            _gatherCommandBtn.OnPressed += GatherCmdPressed;
            _cutTreeCommandBtn.OnPressed += CutTreeCmdPressed;
            _harvestCommandBtn.OnPressed += HarvestCmdPressed;
            _mineCommandBtn.OnPressed += MineCmdPressed;
            _deconstructCommandBtn.OnPressed += DeconstructCmdPressed;
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            SelectionManager.Instance.DeactivateSelectionBox();
            
            _cancelCommandBtn.OnPressed -= CancelCmdPressed;
            _gatherCommandBtn.OnPressed -= GatherCmdPressed;
            _cutTreeCommandBtn.OnPressed -= CutTreeCmdPressed;
            _harvestCommandBtn.OnPressed -= HarvestCmdPressed;
            _mineCommandBtn.OnPressed -= MineCmdPressed;
            _deconstructCommandBtn.OnPressed -= DeconstructCmdPressed;
        }

        #region Button Hooks

        private void CancelCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _cancelCommand, CancelCmdPressed);
        }
        
        private void GatherCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _gatherCommand, GatherCmdPressed);
        }
        
        private void CutTreeCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _cutTreeCommand, CutTreeCmdPressed);
        }
        
        private void HarvestCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _harvestCommand, HarvestCmdPressed);
        }
        
        private void MineCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _mineCommand, MineCmdPressed);
        }
        
        private void DeconstructCmdPressed(ControlsBtn btn)
        {
            HandleCommandSelection(btn, _deconstructCommand, DeconstructCmdPressed);
        }

        #endregion

        private void HandleCommandSelection(ControlsBtn btn, Command command, System.Action<ControlsBtn> onCompleted)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(command, () =>
            {
                // Re-trigger the same command after completion if needed
                onCompleted(btn);
            });
        }
    }
}
