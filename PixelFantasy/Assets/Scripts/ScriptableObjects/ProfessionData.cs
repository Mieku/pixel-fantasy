using Gods;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ProfessionData", menuName = "ProfessionData", order = 1)]
    public class ProfessionData : ScriptableObject
    {
        public string ProfessionName;
        public Sprite ProfessionIcon;
        public ItemData RequiredTool;

        public bool IsRequiredToolAvailable
        {
            get
            {
                if (RequiredTool == null) return true;
                
                return InventoryManager.Instance.IsItemInStorage(RequiredTool);
            }
        }
    }
}
