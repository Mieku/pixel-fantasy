using System;
using ScriptableObjects;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class JobState
    {
        public JobData JobData;
        public bool IsCurrentJob;
        public int TotalExp;

        public JobState(JobData jobData, int totalExp = 0, bool isCurrentJob = false)
        {
            JobData = jobData;
            TotalExp = totalExp;
            IsCurrentJob = isCurrentJob;
        }

        public int CurrentLevel => JobData.DetermineJobLevel(TotalExp);
        public string LevelTitle => JobData.GetLevelName(CurrentLevel);

        public float CurrentLevelProgress()
        {
            int curLvl = CurrentLevel;

            float summedEXP = 0;
            if (curLvl == 1)
            {
                summedEXP = JobData.Lv1Skill.EXPRequired;
                summedEXP += JobData.Lv2Skill.EXPRequired;

                return TotalExp / summedEXP;
            }
            if (curLvl == 2)
            {
                summedEXP = JobData.Lv1Skill.EXPRequired;
                summedEXP += JobData.Lv2Skill.EXPRequired;
                summedEXP += JobData.Lv3Skill.EXPRequired;

                return TotalExp / summedEXP;
            }
            if (curLvl == 3)
            {
                summedEXP = JobData.Lv1Skill.EXPRequired;
                summedEXP += JobData.Lv2Skill.EXPRequired;
                summedEXP += JobData.Lv3Skill.EXPRequired;
                summedEXP += JobData.Lv4Skill.EXPRequired;

                return TotalExp / summedEXP;
            }
            if (curLvl == 4)
            {
                summedEXP = JobData.Lv1Skill.EXPRequired;
                summedEXP += JobData.Lv2Skill.EXPRequired;
                summedEXP += JobData.Lv3Skill.EXPRequired;
                summedEXP += JobData.Lv4Skill.EXPRequired;
                summedEXP += JobData.Lv5Skill.EXPRequired;

                return TotalExp / summedEXP;
            }
            
            // is lvl 5
            return 1.0f;
        }
    }
}
