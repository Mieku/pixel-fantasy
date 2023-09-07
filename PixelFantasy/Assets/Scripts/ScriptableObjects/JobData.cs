using System;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "JobData", menuName = "JobData/JobData", order = 1)]
    public class JobData : ScriptableObject
    {
        public string JobName;
        [Multiline(4)] public string JobDescription;
        public Sprite JobIcon;
        public SkillData Lv1Skill;
        public SkillData Lv2Skill;
        public SkillData Lv3Skill;
        public SkillData Lv4Skill;
        public SkillData Lv5Skill;
        
        [Header("Requirements")]
        public EquipmentData RequiredTool;
        public JobData PrereqJob;
        public int PrereqJobLv;
        

        public bool HasSkills
        {
            get
            {
                if (Lv1Skill.SkillID == "") return false;
                return true;
            }
        }

        public int DetermineJobLevel(int totalExp)
        {
            if (!HasSkills) return 1;
            
            if (totalExp >= Lv5Skill.EXPRequired)
            {
                return 5;
            }
            
            if (totalExp >= Lv4Skill.EXPRequired)
            {
                return 4;
            }
            
            if (totalExp >= Lv3Skill.EXPRequired)
            {
                return 3;
            }
            
            if (totalExp >= Lv2Skill.EXPRequired)
            {
                return 2;
            }

            return 1;
        }
        
        public string GetLevelName(int level)
        {
            if (!HasSkills) return "";

            return level switch
            {
                1 => "Apprentice",
                2 => "Journeyman",
                3 => "Expert",
                4 => "Master",
                5 => "Legendary",
                _ => throw new InvalidOperationException()
            };
        }
    }

    [Serializable]
    public class SkillData
    {
        public string SkillName;
        public string SkillID;
        public Sprite SkillIcon;
        public int EXPRequired;
    }
}
