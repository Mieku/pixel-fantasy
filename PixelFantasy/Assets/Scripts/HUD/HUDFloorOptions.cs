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

        public void BuildWoodFloorBtnPressed()
        {
            // TODO: Build me
        }
    }
}
