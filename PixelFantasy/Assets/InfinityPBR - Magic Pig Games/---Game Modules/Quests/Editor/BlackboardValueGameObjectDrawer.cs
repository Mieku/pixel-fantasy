using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class BlackboardValueGameObjectDrawer : QuestConditionDrawer
    {
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is BlackboardValueGameObject;
        
        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as BlackboardValueGameObject);

        private void ShowData(BlackboardValueGameObject questCondition)
        {
            Label("Blackboard Value Game Object", true);
            StartRow();
            Label($"GameObject {symbolInfo}", "This is the GameObject used in the comparison.", 120);
            questCondition.value = Object(questCondition.value, typeof(GameObject), 200, true) as GameObject;
            EndRow();
            
            StartRow();
            Label($"Value is {symbolInfo}", "This condition will be met if the GameObject value " +
                                            "of the Blackboard Note, when compared to the GameObject value provided here is " +
                                            $"{questCondition.value}", 120);
            questCondition.valueIs = Check(questCondition.valueIs);
            EndRow();
            
            MessageBox("Depending on how this is used, you may need to populate the GameObject value at " +
                       "runtime.", MessageType.Warning);
        }
    }
}