using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace HUD.Cheats
{
    public class HUDCheatResourcesOptions : InputAwareComponent
    {
        private ItemSettings _selectedItemSettings;
        
        public void SpawnResourcePressed(ItemSettings itemSettings)
        {
            _selectedItemSettings = itemSettings;
            PlayerInputController.Instance.ChangeState(PlayerInputState.CHEAT_SpawnResource);
        }
        
        protected override void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            base.GameEvents_OnLeftClickDown(mousePos, inputState, isOverUI);
            
            // if (inputState == PlayerInputState.CHEAT_SpawnResource)
            // {
            //     Spawner.Instance.SpawnItem(_itemDataSelected, UtilsClass.GetMouseWorldPosition(), true);
            // }
        }
        
        protected override void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            base.GameEvents_OnRightClickDown(mousePos, inputState, isOverUI);
            
            CancelCheat();
        }
        
        private void CancelCheat()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            _selectedItemSettings = null;
        }
    }
}
