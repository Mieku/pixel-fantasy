using Data.Structure;
using Systems.Details.Build_Details.Scripts;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class StructureMenu : ControlsMenu
    {
        [SerializeField] private GameObject _subMenuSeperator;
        [SerializeField] private ControlsBtn _wallsBtn;
        [SerializeField] private ControlsBtn _doorsBtn;
        [SerializeField] private ControlsBtn _floorsBtn;
        [SerializeField] private BuildStructureDetailsUI _buildStructureDetails;
        
        public override void Show()
        {
            base.Show();
            
            _buildStructureDetails.Hide();
            _subMenuSeperator.SetActive(true);

            _wallsBtn.OnPressed += WallsPressed;
            _doorsBtn.OnPressed += DoorsPressed;
            _floorsBtn.OnPressed += FloorsPressed;
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            
            _buildStructureDetails.Hide();
            _subMenuSeperator.SetActive(false);
            
            _wallsBtn.OnPressed -= WallsPressed;
            _doorsBtn.OnPressed -= DoorsPressed;
            _floorsBtn.OnPressed -= FloorsPressed;
        }

        private void WallsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildStructureDetails.OnWallPressed();
        }
        
        private void DoorsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildStructureDetails.OnDoorPressed();
        }
        
        private void FloorsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            _subMenuSeperator.SetActive(true);
            _buildStructureDetails.OnFloorPressed();
        }

        public void ShowSpecific(WallSettings wallSettings)
        {
            WallsPressed(_wallsBtn);
            _buildStructureDetails.ShowForSpecificBuild(wallSettings);
        }
        
        public void ShowSpecific(DoorSettings doorSettings)
        {
            DoorsPressed(_doorsBtn);
            _buildStructureDetails.ShowForSpecificBuild(doorSettings);
        }
        
        public void ShowSpecific(FloorSettings floorSettings)
        {
            FloorsPressed(_floorsBtn);
            _buildStructureDetails.ShowForSpecificBuild(floorSettings);
        }
    }
}
