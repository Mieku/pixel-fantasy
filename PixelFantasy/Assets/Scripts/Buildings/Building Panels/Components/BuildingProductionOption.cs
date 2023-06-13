using System;
using HUD.Tooltip;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using Zones;

namespace Buildings.Building_Panels.Components
{
    public class BuildingProductionOption : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _productIcon;
        [SerializeField] private TooltipTrigger _tooltip;

        private Action<CraftedItemData> _onPressed;
        private CraftedItemData _itemData;
        private ProductionZone _room;
        private bool _canBuild;

        private void Start()
        {
            GameEvents.OnRoomFurnitureChanged += GameEvent_OnRoomFurnitureChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnRoomFurnitureChanged -= GameEvent_OnRoomFurnitureChanged;
        }

        private void GameEvent_OnRoomFurnitureChanged(RoomZone room)
        {
            if (room == _room)
            {
                CheckCanCraft();
            }
        }

        public void Init(CraftedItemData itemData, Action<CraftedItemData> onPressed, ProductionZone productionRoom)
        {
            _itemData = itemData;
            _onPressed = onPressed;
            _room = productionRoom;

            _productIcon.sprite = _itemData.ItemSprite;
            _tooltip.Header = _itemData.ItemName;
            
            CheckCanCraft();
        }

        private void CheckCanCraft()
        {
            // Check if the room has the required crafting table
            var requiredTable = _itemData.RequiredCraftingTable;
            if (requiredTable != null)
            {
                if (_room.ContainsFurniture(requiredTable))
                {
                    _canBuild = true;
                    _bg.color = Color.white;
                    _tooltip.Content = "";
                }
                else
                {
                    _canBuild = false;
                    _bg.color = Color.red;
                    _tooltip.Content = $"Room is missing {requiredTable.ItemName}";
                }
            }
            else
            {
                _canBuild = true;
                _bg.color = Color.white;
                _tooltip.Content = "";
            }
        }

        public void OnPressed()
        {
            if (_canBuild)
            {
                _onPressed.Invoke(_itemData);
            }
            else
            {
                Debug.Log($"Room is missing {_itemData.RequiredCraftingTable.ItemName}");
            }
        }
    }
}
