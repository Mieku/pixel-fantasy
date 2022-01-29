using System.Collections.Generic;
using Controllers;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace HUD
{
    public class HUDStructureOptions : InputAwareComponent
    {
        public void SpawnStructurePressed(string key)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStructure, key);
            
            var structureData = Librarian.Instance.GetStructureData(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.StructureData = structureData;
            Spawner.Instance.ShowPlacementIcon(true, structureData.Icon, structureData.InvalidPlacementTags);
        }
    }
}
