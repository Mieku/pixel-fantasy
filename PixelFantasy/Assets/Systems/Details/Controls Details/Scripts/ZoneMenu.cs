using System;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class ZoneMenu : ControlsMenu
    {
        [SerializeField] private ControlsBtn _stockpileZoneBtn;
        [SerializeField] private ControlsBtn _farmingZoneBtn;
        [SerializeField] private GameObject _subMenuSeperator;

        [SerializeField] private StockpileZonesSubMenu _stockpileZonesSubMenu;

        private EMenuType _currentMenu;
        
        private void Start()
        {
            _subMenuSeperator.SetActive(false);
            _stockpileZonesSubMenu.Hide();
            _currentMenu = EMenuType.None;
        }
        
        public override void Show()
        {
            base.Show();

            _stockpileZoneBtn.OnPressed += StockpileZonePressed;
            _farmingZoneBtn.OnPressed += FarmingZonePressed;
            
            SetMenu(EMenuType.None);
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            
            _stockpileZoneBtn.OnPressed -= StockpileZonePressed;
            _farmingZoneBtn.OnPressed -= FarmingZonePressed;
            
            SetMenu(EMenuType.None);
        }

        private void StockpileZonePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            SetMenu(EMenuType.Stockpile);
        }
        
        private void FarmingZonePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            SetMenu(EMenuType.Farming);
        }
        
        private void SetMenu(EMenuType menu)
        {
            switch (_currentMenu)
            {
                case EMenuType.None:
                    break;
                case EMenuType.Stockpile:
                    _stockpileZonesSubMenu.Hide();
                    break;
                case EMenuType.Farming:
                    //_farmingZonesSubMenu.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _currentMenu = menu;
            
            switch (_currentMenu)
            {
                case EMenuType.None:
                    _subMenuSeperator.SetActive(false);
                    break;
                case EMenuType.Stockpile:
                    _subMenuSeperator.SetActive(true);
                    _stockpileZonesSubMenu.Show();
                    break;
                case EMenuType.Farming:
                    _subMenuSeperator.SetActive(true);
                    //_farmingZonesSubMenu.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum EMenuType
        {
            None,
            Stockpile,
            Farming,
        }
    }
}
