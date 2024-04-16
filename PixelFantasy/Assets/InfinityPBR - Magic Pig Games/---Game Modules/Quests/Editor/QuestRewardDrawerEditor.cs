using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class QuestRewardDrawerEditor : QuestRewardDrawer
    {
        private int _fieldWidth;
        private Vector2 _scrollPosition;
        //private int _addSourceIndex = -1;
        //private int _sourceCalculation = 0;
        
        public QuestRewardDrawerEditor(int fieldWidth) => _fieldWidth = fieldWidth;

        public QuestRewardDrawerEditor()
        {
            // Any initialization code goes here
        }

        protected override void ShowSpecificData(QuestReward questReward)
        {
            throw new NotImplementedException();
        }

        public void showData(QuestReward questReward)
        {
            
        }

        // ------------------------------------------------------------------------------------------------
        // COMMON
        // ------------------------------------------------------------------------------------------------
        
    }
}