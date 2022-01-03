using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace HUD.Cheats
{
    public class HUDCheatResourcesOptions : InputAwareComponent
    {
        private ItemData _itemDataSelected;

        public GameObject wallPrefab;
        public StructureData structureData;
        
        public void SpawnResourcePressed(ItemData itemData)
        {
            _itemDataSelected = itemData;
            PlayerInputController.Instance.ChangeState(PlayerInputState.CHEAT_SpawnResource);
        }

        public void SpawnWallPressed()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.CHEAT_SpawnWall);
        }
        
        protected override void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnLeftClickDown(mousePos, inputState);
            
            if (inputState == PlayerInputState.CHEAT_SpawnResource)
            {
                ItemSpawner.Instance.SpawnItem(_itemDataSelected, UtilsClass.GetMouseWorldPosition(), true);
            }

            if (inputState == PlayerInputState.CHEAT_SpawnWall)
            {
                SpawnWall(Helper.ConvertMousePosToGridPos(mousePos));
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

        private void SpawnWall(Vector2 pos)
        {
            ItemSpawner.Instance.SpawnStructure(structureData, pos);
        }
    }
}
