using System;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class ControlsDetails : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _seperator;
        [SerializeField] private ControlsBtn _commandsBtn;
        [SerializeField] private ControlsBtn _furnitureBtn;
        [SerializeField] private ControlsBtn _structureBtn;
        [SerializeField] private ControlsBtn _zonesBtn;
        [SerializeField] private ControlsBtn _jobPrioritiesBtn;
        [SerializeField] private ControlsBtn _scheduleBtn;

        [SerializeField] private CommandMenu _commandMenu;
        [SerializeField] private FurnitureMenu _furnitureMenu;
        [SerializeField] private StructureMenu _structureMenu;
        [SerializeField] private ZoneMenu _zoneMenu;
        [SerializeField] private JobPrioritiesMenu _jobPrioritiesMenu;
        [SerializeField] private ScheduleMenu _scheduleMenu;

        private ControlsBtn _activeBtn;
        private EMenuType _currentMenu;

        private void Start()
        {
            _seperator.SetActive(false);
            _commandMenu.Hide();
            _furnitureMenu.Hide();
            _structureMenu.Hide();
            _zoneMenu.Hide();
            _currentMenu = EMenuType.None;
        }

        public void Show()
        {
            _panel.SetActive(true);
            
            _commandsBtn.OnPressed += CommandsPressed;
            _furnitureBtn.OnPressed += FurniturePressed;
            _structureBtn.OnPressed += StructurePressed;
            _zonesBtn.OnPressed += ZonesPressed;
            _jobPrioritiesBtn.OnPressed += JobPrioritiesPressed;
            _scheduleBtn.OnPressed += SchedulePressed;
            
            SetMenu(EMenuType.None);
        }

        public void Hide()
        {
            SetBtnActive(null);
            SetMenu(EMenuType.None);
            
            _commandsBtn.OnPressed -= CommandsPressed;
            _furnitureBtn.OnPressed -= FurniturePressed;
            _structureBtn.OnPressed -= StructurePressed;
            _zonesBtn.OnPressed -= ZonesPressed;
            _jobPrioritiesBtn.OnPressed -= JobPrioritiesPressed;
            _scheduleBtn.OnPressed -= SchedulePressed;
            
            _panel.SetActive(false);
        }

        public void OpenBuildStructure(WallSettings wallSettings)
        {
            StructurePressed(_structureBtn);
            _structureMenu.ShowSpecific(wallSettings);
        }
        
        public void OpenBuildStructure(DoorSettings doorSettings)
        {
            StructurePressed(_structureBtn);
            _structureMenu.ShowSpecific(doorSettings);
        }
        
        public void OpenBuildStructure(FloorSettings floorSettings)
        {
            StructurePressed(_structureBtn);
            _structureMenu.ShowSpecific(floorSettings);
        }

        #region Button Hooks

        private void CommandsPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.Commands);
        }

        private void FurniturePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.Furniture);
        }

        private void StructurePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.Structure);
        }

        private void ZonesPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.Zones);
        }

        private void JobPrioritiesPressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.JobPriorities);
        }
        
        private void SchedulePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            
            SetMenu(EMenuType.Schedule);
        }

        private void SetBtnActive(ControlsBtn btn)
        {
            if (_activeBtn != null)
            {
                _activeBtn.SetActive(false);
            }

            _activeBtn = btn;
            
            if (_activeBtn != null)
            {
                _activeBtn.SetActive(true);
            }
        }

        #endregion

        private void SetMenu(EMenuType menu)
        {
            switch (_currentMenu)
            {
                case EMenuType.None:
                    break;
                case EMenuType.Commands:
                    _commandMenu.Hide();
                    break;
                case EMenuType.Furniture:
                    _furnitureMenu.Hide();
                    break;
                case EMenuType.Structure:
                    _structureMenu.Hide();
                    break;
                case EMenuType.Zones:
                    _zoneMenu.Hide();
                    break;
                case EMenuType.JobPriorities:
                    _jobPrioritiesMenu.Hide();
                    break;
                case EMenuType.Schedule:
                    _scheduleMenu.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _currentMenu = menu;
            
            switch (_currentMenu)
            {
                case EMenuType.None:
                    _seperator.SetActive(false);
                    break;
                case EMenuType.Commands:
                    _seperator.SetActive(true);
                    _commandMenu.Show();
                    break;
                case EMenuType.Furniture:
                    _seperator.SetActive(true);
                    _furnitureMenu.Show();
                    break;
                case EMenuType.Structure:
                    _seperator.SetActive(true);
                    _structureMenu.Show();
                    break;
                case EMenuType.Zones:
                    _seperator.SetActive(true);
                    _zoneMenu.Show();
                    break;
                case EMenuType.JobPriorities:
                    _seperator.SetActive(true);
                    _jobPrioritiesMenu.Show();
                    break;
                case EMenuType.Schedule:
                    _seperator.SetActive(true);
                    _scheduleMenu.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum EMenuType
        {
            None,
            Commands,
            Furniture,
            Structure,
            Zones,
            JobPriorities,
            Schedule,
        }
    }
}
