using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TaskSystem;
using UnityEngine;

namespace Systems.Skills.Scripts
{
    public class KinlingSkills : MonoBehaviour
    {
        [ShowInInspector] private List<TalentSO> _talents = new List<TalentSO>();
        [ShowInInspector] private List<Skill> _gearSkills = new List<Skill>();

        [SerializeField] private Color _relevantStatColour;
        
        public List<Skill> AllSkills
        {
            get
            {
                List<Skill> allSkills = new List<Skill>();
                foreach (var talent in _talents)
                {
                    allSkills.AddRange(new List<Skill>(talent.Skills));
                }

                foreach (var gearSkill in _gearSkills)
                {
                    allSkills.Add(new Skill(gearSkill.SkillType, gearSkill.Amount));
                }

                List<Skill> combinedSkills = new List<Skill>();
                foreach (var skill in allSkills)
                {
                    var exisitingSkill = combinedSkills.Find(s => s.SkillType == skill.SkillType);
                    if (exisitingSkill == null)
                    {
                        combinedSkills.Add(new Skill(skill.SkillType, skill.Amount));
                    }
                    else
                    {
                        exisitingSkill.Amount += skill.Amount;
                    }
                }

                var orderedSkills = combinedSkills.OrderByDescending(skill => (skill.Amount)).ToList();

                return orderedSkills;
            }
        }

        public string GetSkillList(SkillType relevantSkill = SkillType.None, Color relevantColourOverride = default)
        {
            string result = "";
            Color relevantColour = _relevantStatColour;
            if (relevantColourOverride != default)
            {
                relevantColour = relevantColourOverride;
            }

            var allSkills = AllSkills;
            if (relevantSkill != SkillType.None)
            {
                var topSkill = allSkills.Find(s => s.SkillType == relevantSkill);
                if (topSkill != null)
                {
                    result += $"<color={Helper.ColorToHex(relevantColour)}>{topSkill}</color><br>";
                }
            }

            foreach (var skill in allSkills)
            {
                if (skill.SkillType != relevantSkill)
                {
                    result += $"{skill}<br>";
                }
            }

            return result;
        }

        public int GetTotalSkill(SkillType skillType)
        {
            return (from skill in AllSkills where skill.SkillType == skillType select skill.Amount).FirstOrDefault();
        }

        public float GetWorkAmount(ETaskType taskType)
        {
            // float defaultChangePerLevel = 0.1f;
            // if (skillType == SkillType.None)
            // {
            //     return 1;
            // }
            //
            // var totalSkill = GetTotalSkill(skillType);
            // if (totalSkill <= 0)
            // {
            //     return 0.5f;
            // }
            //
            // return 1 + (defaultChangePerLevel * totalSkill);
            
            // TODO: Needs redesign
            return 1;
        }

        public bool DoSkillRoll(SkillType skillType, float baseChance, int minSkillForSuccess = 0)
        {
            var totalSkill = GetTotalSkill(skillType);
            if (totalSkill <= minSkillForSuccess)
            {
                return false;
            }

            float defaultChangePerLevel = 0.1f;
            var skillChance = defaultChangePerLevel * totalSkill;
            var chance = baseChance + (baseChance * skillChance);
            return Helper.RollDice(chance);
        }

        public void Init(List<TalentSO> talents)
        {
            _talents.AddRange(talents);
        }

        public void ApplyGearSkills(List<Skill> skills)
        {
            _gearSkills.AddRange(skills);
        }

        public void RemoveGearSkills(List<Skill> skills)
        {
            foreach (var skill in skills)
            {
                if (_gearSkills.Contains(skill))
                {
                    _gearSkills.Remove(skill);
                }
                else
                {
                    Debug.LogError($"Attempted to remove a not existing skill: {skill.SkillType.GetDescription()}");
                }
            }
        }
    }
}
