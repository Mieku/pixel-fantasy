using System.Collections.Generic;
using Controllers;
using Gods;
using UnityEngine;

namespace HUD
{
    public class HUDFloorOptions : InputAwareComponent
    {
        [SerializeField] private DirtTile _dirtPrefab;

        public void ClearGrassBtnPressed()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFlooring, "Dirt");
            Spawner.Instance.ShowPlacementIcon(true, _dirtPrefab.Icon, _dirtPrefab.InvalidPlacementTags);
        }

        public void BuildFloorBtnPressed(string key)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFlooring, key);
            
            var floorData = Librarian.Instance.GetFloorData(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.FloorData = floorData;
            Spawner.Instance.ShowPlacementIcon(true, floorData.Icon, floorData.InvalidPlacementTags);
        }
    }
}
