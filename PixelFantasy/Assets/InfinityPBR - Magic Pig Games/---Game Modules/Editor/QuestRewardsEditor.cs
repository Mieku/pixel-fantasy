using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CanEditMultipleObjects]
    [Serializable]
    public class QuestRewardsEditor : Editor
    {
        private QuestReward _modulesObject;
        private QuestRewardsDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (QuestReward)target;
            _modulesDrawer = CreateInstance<QuestRewardsDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
    }
}