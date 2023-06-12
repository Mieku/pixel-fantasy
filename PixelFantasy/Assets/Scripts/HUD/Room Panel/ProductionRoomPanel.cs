using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zones;

namespace HUD.Room_Panel
{
    public class ProductionRoomPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_InputField _zoneNameText;
        [SerializeField] private Image _prodBtnBG, _workersBtnBG, _infoBtnBG;
        [SerializeField] private Image _buildBtnBG, _inventoryBtnBG;
        [SerializeField] private Sprite _activeBtnSprite, _inactiveBtnSprite;
        [SerializeField] private ProductionPanel _productionPanel;
        [SerializeField] private WorkersPanel _workersPanel;
        [SerializeField] private InfoPanel _infoPanel;
        [SerializeField] private BuildPanel _buildPanel;
        [SerializeField] private RoomInventoryPanel _inventoryPanel;

        private ProductionZone _zone;
        private RoomPanelTopState _topState = RoomPanelTopState.Production;
        private RoomPanelBottomState _bottomState = RoomPanelBottomState.Build;

        public void Show(ProductionZone zone)
        {
            _zone = zone;
            _root.SetActive(true);
            _zoneNameText.text = _zone.Name;
            ShowTopState(_topState);
            ShowBottomState(_bottomState);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        // Triggered from input field
        public void EditZoneName()
        {
            var nameInput = _zoneNameText.text;
            Debug.Log("Edited Name to be: " + nameInput);
            _zone.EditZoneName(nameInput);
        }

        public void DeleteRoomPressed()
        {
            // TODO: Add an are you sure?
            _zone.RemoveZone();
            Hide();
        }

        public void ProductionBtnPressed()
        {
            _topState = RoomPanelTopState.Production;
            ShowTopState(RoomPanelTopState.Production);
        }

        public void WorkersBtnPressed()
        {
            _topState = RoomPanelTopState.Workers;
            ShowTopState(RoomPanelTopState.Workers);
        }

        public void InfoBtnPressed()
        {
            _topState = RoomPanelTopState.Info;
            ShowTopState(RoomPanelTopState.Info);
        }

        public void BuildBtnPressed()
        {
            _bottomState = RoomPanelBottomState.Build;
            ShowBottomState(RoomPanelBottomState.Build);
        }

        public void InventoryBtnPressed()
        {
            _bottomState = RoomPanelBottomState.Inventory;
            ShowBottomState(RoomPanelBottomState.Inventory);
        }

        private void ClearTopBtnsActive()
        {
            _prodBtnBG.sprite = _inactiveBtnSprite;
            _workersBtnBG.sprite = _inactiveBtnSprite;
            _infoBtnBG.sprite = _inactiveBtnSprite;
            
            _productionPanel.Hide();
            _workersPanel.Hide();
            _infoPanel.Hide();
        }

        private void ClearBottomBtnsActive()
        {
            _buildBtnBG.sprite = _inactiveBtnSprite;
            _inventoryBtnBG.sprite = _inactiveBtnSprite;
            
            _buildPanel.Hide();
            _inventoryPanel.Hide();
        }

        private void ShowTopState(RoomPanelTopState state)
        {
            ClearTopBtnsActive();
            
            switch (state)
            {
                case RoomPanelTopState.Production:
                    _prodBtnBG.sprite = _activeBtnSprite;
                    _productionPanel.Show(_zone);
                    break;
                case RoomPanelTopState.Workers:
                    _workersBtnBG.sprite = _activeBtnSprite;
                    _workersPanel.Show(_zone);
                    break;
                case RoomPanelTopState.Info:
                    _infoBtnBG.sprite = _activeBtnSprite;
                    _infoPanel.Show(_zone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void ShowBottomState(RoomPanelBottomState state)
        {
            ClearBottomBtnsActive();
            
            switch (state)
            {
                case RoomPanelBottomState.Build:
                    _buildBtnBG.sprite = _activeBtnSprite;
                    _buildPanel.Show(_zone);
                    break;
                case RoomPanelBottomState.Inventory:
                    _inventoryBtnBG.sprite = _activeBtnSprite;
                    _inventoryPanel.Show(_zone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public enum RoomPanelTopState
        {
            Production,
            Workers,
            Info,
        }

        public enum RoomPanelBottomState
        {
            Build,
            Inventory,
        }
    }
}
