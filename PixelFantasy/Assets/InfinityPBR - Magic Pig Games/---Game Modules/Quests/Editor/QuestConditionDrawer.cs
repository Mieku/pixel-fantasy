using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public abstract class QuestConditionDrawer : InfinityEditor
    {
        public virtual bool CanHandle(QuestCondition questCondition)
        {
            throw new NotImplementedException();
        }
        
        public void Draw(QuestCondition questCondition)
        {
            if (questCondition == null)
                return;

            EditorGUI.BeginChangeCheck();

            var type = questCondition.GetType().ToString().Replace("InfinityPBR.Modules.", "");

            StartVerticalBox();
            
            if (!questCondition.drawnByGameModulesManager)
            {
                StartRow();

                ButtonOpenClose($"Editor Show Quest Condition {questCondition.name}", ShowCondition(questCondition));

                Label($"{questCondition.objectName}");
                LabelGrey($"{type}");
                EndRow();
            }

            // Stop here if we aren't showing this condition
            if (!ShowCondition(questCondition))
            {
                EndVerticalBox();
                return;
            }

            if (!questCondition.drawnByGameModulesManager)
                Space();
            ShowCommonData(questCondition);

            Space();
            ShowSpecificData(questCondition);

            EndVerticalBox();

            ShowCodeExamples(questCondition);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(questCondition);
            }
        }
        
        private bool ShowCondition(QuestCondition questCondition) => GetBool($"Editor Show Quest Condition {questCondition.name}");

        private void ShowCommonData(QuestCondition questCondition)
        {
            //Undo.RecordObject(questCondition, "Change Quest Condition Name");
            Undo.RecordObject(questCondition, "Update Name");
            ShowQuestConditionName(questCondition);
            
            Undo.RecordObject(questCondition, "Change Quest Condition Description");
            ShowQuestConditionDescription(questCondition);

            Undo.RecordObject(questCondition, "Change Quest Condition Blackboard Values");
            ShowQuestConditionBlackboardValues(questCondition);
        }
        
        private void ShowQuestConditionBlackboardValues(QuestCondition questCondition)
        {
            Label($"Blackboard Values {symbolInfo}", "This condition will lookup the Blackboard values based on the " +
                                                     "topic and subject set here. Be sure to set the Blackboard value in your " +
                                                     "code or this condition will never resolve to true.", true);
            StartRow();
            Label("Topic", 120);
            if (questCondition.gameIdAsTopic)
            {
                if (Button(GetBool("Condition Show Code") ? "Hide Code Example" : "Show Code Example", 200))
                    ToggleBool("Condition Show Code");
            }
            else
                questCondition.topic = TextField(questCondition.topic, 200);
            questCondition.gameIdAsTopic = LeftCheck($"Use Game Id {symbolInfo}", "If true, this will use the value from GameId(), a required " +
                                                     "method on any object which implements IHaveQuests. This allows the " +
                                                     "Quest Condition to be used without knowing in advance which IHaveQuest objects " +
                                                     "may be using it.\n\nSee the code example that appears when true, or visit the " +
                                                     "online docs linked from InfinityPBR.com for more info.", questCondition.gameIdAsTopic);
            EndRow();
            StartRow();
            Label("Subject", 120);
            if (questCondition.questNameQuestStepAsSubject)
            {
                if (Button(GetBool("Condition Show Code") ? "Hide Code Example" : "Show Code Example", 200))
                    ToggleBool("Condition Show Code");
            }
            else
                questCondition.subject = TextField(questCondition.subject, 200);
            questCondition.questNameQuestStepAsSubject = LeftCheck($"Use Quest Name/Step {symbolInfo}", "If true, this will use the Quest Name and Quest Step Name as " +
                "the \"Subject\". This allows the " +
                "Quest Condition to be used without knowing in advance which Quest/Step " +
                "may be using it.\n\nSee the code example that appears when true, or visit the " +
                "online docs linked from InfinityPBR.com for more info.\n\nIMPORTANT: The name will be \"QuestName-QuestStepName\" with " +
                "a dash between the two, no space.", questCondition.questNameQuestStepAsSubject);
            EndRow();
        }

        private void ShowQuestConditionDescription(QuestCondition questCondition)
        {
            StartRow();
            Label($"Description {symbolInfo}", "Use this description in your project, or just as information for " +
                                               "yourself.", 120);
            questCondition.description = TextArea(questCondition.description, 200, 50);
            EndRow();
        }

        private void ShowQuestConditionName(QuestCondition questCondition)
        {
            StartRow();
            Label($"Name {symbolInfo}", "This is the name of the Quest Condition, and also the Scriptable Object. Quest" +
                                        " Condition names must be unique.", 120);
            var tempName = questCondition.objectName;
            questCondition.objectName = DelayedText(questCondition.objectName, 200);
            if (tempName != questCondition.objectName)
            {
                // Check for unique name
                if (GameModuleObjects<QuestCondition>().Count(x => x.objectName == questCondition.objectName) > 1)
                {
                    Debug.LogWarning($"There is already a Quest Condition named {questCondition.objectName}.");
                    questCondition.objectName = tempName;
                    EndRow();
                    return;
                }
                
                // Change the scriptable object name
                questCondition.ChangeName(questCondition.objectName);

                // Update the bool to show this panel
                SetBool($"Editor Show Quest Condition {questCondition.objectName}", true);
            }
            EndRow();
        }
        
        private void ShowCodeExamples(QuestCondition questCondition)
        {
            if (!questCondition.gameIdAsTopic && !questCondition.questNameQuestStepAsSubject)
                return;

            if (!GetBool("Condition Show Code")) return;

            Space();
            StartVerticalBox();
            Header1("Code Examples");
            ShowTopicCode(questCondition);
            ShowSubjectCode(questCondition);
            ShowFinalCode(questCondition);
            EndVerticalBox();
        }
        
        private void ShowFinalCode(QuestCondition questCondition)
        {
            var topicToUse = questCondition.topic;
            var subjectToUse = questCondition.subject;
            
            if (questCondition.gameIdAsTopic)
                topicToUse = "ObjectId()";
            if (questCondition.questNameQuestStepAsSubject)
                subjectToUse = "[QuestName-QuestStep]";
            
            Space();
            Header2("CODE EXAMPLE");
            Label("Based on the Subject above, when the object sends data to the Blackboard, it can use:\n\n" +
                  $"blackboard.UpdateNote({topicToUse}, \"{subjectToUse}\", [VALUE WILL GO HERE]);", false, true);
        }

        private void ShowTopicCode(QuestCondition questCondition)
        {
            if (!questCondition.gameIdAsTopic) return;

            var subjectToUse = questCondition.subject;
            if (questCondition.questNameQuestStepAsSubject)
                subjectToUse = "[QuestName-QuestStep]";

            Space();
            Header2("TOPIC");
            Label("The object which has quests needs to implement IHaveQuests. It will then have a method GameId() which returns a string.\n\n" +
                  "Any Blackboard Notes which refer to the owner of this object should use GameId() as the \"Topic\"" +
                  "of the note.", false, true);
        }

        private void ShowSubjectCode(QuestCondition questCondition)
        {
            if (!questCondition.questNameQuestStepAsSubject) return;
            
            Space();
            Header2("SUBJECT");
            Label("The subject will be automatically populated based on the Quest name and QuestStep name. The demo scene " +
                  "for the Quests demo has an example quest called \"Don't Get Poisoned\" with a single step called \"Stay Healthy\". " +
                  "If this QuestCondition were attached to the \"Stay Healthy\" QuestStep, then the subject for the Blackboard Value " +
                  "lookup would be \"Don't Get Poisoned-Stay Healthy\".\n\nThis is the subject you would need to use when sending data " +
                  "to the blackboard in your custom code.", false, true);
        }
        
        protected abstract void ShowSpecificData(QuestCondition questCondition);
    }
}