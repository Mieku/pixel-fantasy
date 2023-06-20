using System;
using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class BuildPanel : MonoBehaviour
    {
        [SerializeField] private BuildPanelSlot _slotPrefab;
        [SerializeField] private Transform _slotParent;
        
        private RoomZone _zone;
        private List<BuildPanelSlot> _displayedSlots = new List<BuildPanelSlot>();
        private BuildPanelSlot _curPressedSlot;

        public void Show(RoomZone zone)
        {
            _zone = zone;
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            GameEvents.OnRightClickUp += OnRightClickPressed;
        }

        private void OnDestroy()
        {
            GameEvents.OnRightClickUp -= OnRightClickPressed;
        }

        private void Refresh()
        {
            foreach (var displayedSlot in _displayedSlots)
            {
                Destroy(displayedSlot.gameObject);
            }
            _displayedSlots.Clear();
            
            var buildOptions = _zone.BuildFurnitureOptions();
            foreach (var buildOption in buildOptions)
            {
                var displayOption = Instantiate(_slotPrefab, _slotParent);
                displayOption.Init(buildOption, OnSlotPressed);
                _displayedSlots.Add(displayOption);
            }
        }

        private void OnSlotPressed(FurnitureItemData furnitureItemData, BuildPanelSlot slot)
        {
            Spawner.Instance.CancelInput();
            
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, furnitureItemData.ItemName);
            Spawner.Instance.PlanFurniture(furnitureItemData);

            if (_curPressedSlot != null)
            {
                _curPressedSlot.DisplayActive(false);
            }

            _curPressedSlot = slot;
            _curPressedSlot.DisplayActive(true);
        }

        private void OnRightClickPressed( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
        {
            if (_curPressedSlot != null)
            {
                Spawner.Instance.CancelInput();
                _curPressedSlot.DisplayActive(false);
            }

            _curPressedSlot = null;
        }
    }
}
