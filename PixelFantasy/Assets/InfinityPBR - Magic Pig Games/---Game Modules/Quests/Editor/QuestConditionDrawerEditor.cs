using System;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class QuestConditionDrawerEditor : QuestConditionDrawer
    {
        private int _fieldWidth;
        private Vector2 _scrollPosition;
        //private int _addSourceIndex = -1;
        //private int _sourceCalculation = 0;
        private LookupTable[] _availableLookupTables;
        private string[] _availableLookupTablesNames;

        private BlackboardNote[] _blackboardNotes;
        private BlackboardNote[] _blackboardNotesOfSelectedTopic;
        private BlackboardNote[] BlackboardNotesOfTopic(string topic) =>
            _blackboardNotes.Where(x => x.topic == topic).ToArray();

        private string[] BlackboardNotesOfTopicNames(string topic) =>
            BlackboardNotesOfTopic(topic).Select(x => x.subject).ToArray();

        private BlackboardNote _selectedBlackboardNote;
        //private int _selectedNoteIndex = 0;
        private string[] _blackboardNoteTopics;
        private string _selectedTopic = "";
        //private int _selectedTopicIndex = 0;

        private int AvailableTopics => _blackboardNoteTopics.Length;
        private QuestCondition _thisCondition;
        private QuestStep _thisStep;

        private void CacheSelectedTopic() => _blackboardNotesOfSelectedTopic = BlackboardNotesOfTopic(_selectedTopic);

        public QuestConditionDrawerEditor(int fieldWidth) => _fieldWidth = fieldWidth;
        
        public QuestConditionDrawerEditor()
        {
            // Any initialization code goes here
        }

        //private bool madeCache = false;

        private bool ShowCondition(QuestCondition questCondition) => GetBool($"Editor Show Quest Condition {questCondition.name}");
        
        protected override void ShowSpecificData(QuestCondition questCondition)
        {
            throw new NotImplementedException();
        }
    }
}