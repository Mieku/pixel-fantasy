using CodeMonkey.Utils;
using Controllers;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace HUD.Cheats
{
    public class HUDCheatResourcesOptions : InputAwareComponent
    {
        private ItemData _itemDataSelected;
        
        public void SpawnResourcePressed(ItemData itemData)
        {
            _itemDataSelected = itemData;
            PlayerInputController.Instance.ChangeState(PlayerInputState.CHEAT_SpawnResource);
        }
        
        protected override void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnLeftClickDown(mousePos, inputState);
            
            if (inputState == PlayerInputState.CHEAT_SpawnResource)
            {
                Spawner.Instance.SpawnItem(_itemDataSelected, UtilsClass.GetMouseWorldPosition(), true);
            }
        }
        
        protected override void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnRightClickDown(mousePos, inputState);
            
            CancelCheat();
        }
        
        private void CancelCheat()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            _itemDataSelected = null;
        }
    }
}
