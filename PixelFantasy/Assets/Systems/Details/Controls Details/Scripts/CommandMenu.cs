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
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_cancelCommand, () =>
            {
                CancelCmdPressed(btn);
            });
        }
        
        private void GatherCmdPressed(ControlsBtn btn)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_gatherCommand, () =>
            {
                GatherCmdPressed(btn);
            });
        }
        
        private void CutTreeCmdPressed(ControlsBtn btn)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_cutTreeCommand, () =>
            {
                CutTreeCmdPressed(btn);
            });
        }
        
        private void HarvestCmdPressed(ControlsBtn btn)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_harvestCommand, () =>
            {
                HarvestCmdPressed(btn);
            });
        }
        
        private void MineCmdPressed(ControlsBtn btn)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_mineCommand, () =>
            {
                MineCmdPressed(btn);
            });
        }
        
        private void DeconstructCmdPressed(ControlsBtn btn)
        {
            SelectionManager.Instance.DeactivateSelectionBox();
            SetBtnActive(btn);
            
            SelectionManager.Instance.BeginCommandSelectionBox(_deconstructCommand, () =>
            {
                DeconstructCmdPressed(btn);
            });
        }

        #endregion
    }
}
