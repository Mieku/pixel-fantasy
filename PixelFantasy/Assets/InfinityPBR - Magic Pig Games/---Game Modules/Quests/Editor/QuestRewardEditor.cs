using System;
using UnityEditor;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(QuestReward))]
    [CanEditMultipleObjects]
    [Serializable]
    public abstract class QuestRewardEditor : ModulesScriptableObjectEditor
    {
        private QuestReward _this;

        protected QuestRewardDrawer questRewardDrawer;

        //private float _width;
        //private int _columnWidth = 190;
        //private Vector2 _scrollPosition;

        // Abstract method for creating a drawer
        protected abstract QuestRewardDrawer CreateDrawer();
        
        protected override void Setup()
        {
            _this = (QuestReward) target;
            questRewardDrawer = CreateDrawer();
        }

        protected override void Header()
        {
            
        }

        protected override void Draw()
        {
            CheckName();
            questRewardDrawer.Draw(_this);

            Space();
            FoldoutSetBool("Draw Default Inspector Quest Reward", "Draw Default Inspector");
            if (!GetBool("Draw Default Inspector Quest Reward")) return;
            
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