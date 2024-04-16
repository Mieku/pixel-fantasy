using System;
using UnityEditor;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(QuestCondition))]
    [CanEditMultipleObjects]
    [Serializable]
    public abstract class QuestConditionEditor : ModulesScriptableObjectEditor
    {
        private QuestCondition _this;

        protected QuestConditionDrawer questConditionDrawer;

        //private float _width;
        //private int _columnWidth = 190;
        //private Vector2 _scrollPosition;
        
        // Abstract method for creating a drawer
        protected abstract QuestConditionDrawer CreateDrawer();

        protected override void Setup()
        {
            _this = (QuestCondition) target;
            questConditionDrawer = CreateDrawer();
        }

        protected override void Header()
        {
            
        }

        protected override void Draw()
        {
            CheckName();
            questConditionDrawer.Draw(_this);

            Space();
            FoldoutSetBool("Draw Default Inspector Quest Condition", "Draw Default Inspector");
            if (!GetBool("Draw Default Inspector Quest Condition")) return;
            
            DrawDefaultInspector();
        }
        
        protected void CheckName()
        {
            if (!string.IsNullOrWhiteSpace(_this.objectName)
                && _this.objectName == _this.name) return;

            _this.objectName = _this.name;
        }
    }
}