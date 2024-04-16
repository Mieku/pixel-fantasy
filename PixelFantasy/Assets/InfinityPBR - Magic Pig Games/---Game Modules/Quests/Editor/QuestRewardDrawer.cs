using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public abstract class QuestRewardDrawer : InfinityEditor
    {
        public virtual bool CanHandle(QuestReward questReward)
        {
            throw new NotImplementedException();
        }

        private bool ShowReward(QuestReward questReward) => GetBool($"Editor Show Quest Reward {questReward.name}");
        
        public void Draw(QuestReward questReward)
        {
            if (questReward == null)
                return;

            EditorGUI.BeginChangeCheck();
            
            var type = questReward.GetType().ToString().Replace("InfinityPBR.Modules.", "");
            
            StartVerticalBox();
            

            if (!questReward.drawnByGameModulesManager)
            {
                StartRow();
                ButtonOpenClose($"Editor Show Quest Reward {questReward.name}", ShowReward(questReward));

                Label($"{questReward.objectName}");
                LabelGrey($"{type}");
                EndRow();
            }

            // Stop here if we aren't showing this reward
            if (!ShowReward(questReward))
            {
                EndVerticalBox();
                return;
            }
    
            if (!questReward.drawnByGameModulesManager)
                Space();
            ShowCommonData(questReward);
            
            Space();

            ShowSpecificData(questReward);

            EndVerticalBox();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(questReward);
            }
        }
        
        protected abstract void ShowSpecificData(QuestReward questReward);

        protected virtual void OnEnable()
        {
           
        }

        protected virtual void DoCache()
        {
            
        }
        
        private void ShowCommonData(QuestReward questReward)
        {
            Undo.RecordObject(questReward, "Update Name");
            ShowQuestRewardName(questReward);
            
            Undo.RecordObject(questReward, "Change Quest Reward Description");
            ShowQuestRewardDescription(questReward);
        }
        
        private void ShowQuestRewardDescription(QuestReward questReward)
        {
            StartRow();
            Label($"Description {symbolInfo}", "Use this description in your project, or just as information for " +
                                               "yourself.", 120);
            questReward.description = TextArea(questReward.description, 200, 50);
            EndRow();
        }

        private void ShowQuestRewardName(QuestReward questReward)
        {
            StartRow();
            Label($"Name {symbolInfo}", "This is the name of the Quest Reward, and also the Scriptable Object. Quest" +
                                        " Reward names must be unique.", 120);
            var tempName = questReward.objectName;
            questReward.objectName = DelayedText(questReward.objectName, 200);
            if (tempName != questReward.objectName)
            {
                // Check for unique name
                if (GameModuleObjects<QuestCondition>().Count(x => x.objectName == questReward.objectName) > 1)
                {
                    Debug.LogWarning($"There is already a Quest Reward named {questReward.objectName}.");
                    questReward.objectName = tempName;
                    EndRow();
                    return;
                }
                
                // Change the scriptable object name
                questReward.ChangeName(questReward.objectName);

                // Update the bool to show this panel
                SetBool($"Editor Show Quest Reward {questReward.objectName}", true);
            }
            EndRow();
        }
    }
}