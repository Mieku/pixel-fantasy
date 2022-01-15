using Controllers;
using Gods;
using UnityEngine;

namespace HUD
{
    public class HUDStructureOptions : MonoBehaviour
    {
        public void SpawnStructurePressed(string key)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStructure, key);
            
            var structureData = Librarian.Instance.GetStructureData(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.ShowPlacementIcon(true, structureData.Icon, structureData.InvalidPlacementTags);
        }
    }
}
