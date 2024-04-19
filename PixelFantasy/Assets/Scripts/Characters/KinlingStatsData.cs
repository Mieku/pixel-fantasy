using System;
using System.Collections.Generic;
using Databrain.Attributes;
using ScriptableObjects;
using Systems.Stats.Scripts;
using TaskSystem;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class KinlingStatsData
    {
        [ExposeToInspector, DatabrainSerialize]
        public SkillData MeleeSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData RangedSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData ConstructionSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData MiningSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData BotanySkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData CookingSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData CraftingSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData BeastMasterySkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData MedicalSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData SocialSkill;
        
        [ExposeToInspector, DatabrainSerialize]
        public SkillData IntelligenceSkill;

        [ExposeToInspector, DatabrainSerialize]
        public List<Trait> Traits = new List<Trait>();

        public SkillData GetSkillByType(ESkillType skillType)
        {
            switch (skillType)
            {
                case ESkillType.Mining:
                    return MiningSkill;
                case ESkillType.Cooking:
                    return CookingSkill;
                case ESkillType.Melee:
                    return MeleeSkill;
                case ESkillType.Ranged:
                    return RangedSkill;
                case ESkillType.Construction:
                    return ConstructionSkill;
                case ESkillType.Botany:
                    return BotanySkill;
                case ESkillType.Crafting:
                    return CraftingSkill;
                case ESkillType.BeastMastery:
                    return BeastMasterySkill;
                case ESkillType.Medical:
                    return MedicalSkill;
                case ESkillType.Social:
                    return SocialSkill;
                case ESkillType.Intelligence:
                    return IntelligenceSkill;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
        
        public int GetLevelForSkill(ESkillType skillType)
        {
            var skill = GetSkillByType(skillType);
            return skill.Level;
        }
        
        public void SetLevelForSkill(ESkillType skillType, int assignedLevel)
        {
            var skill = GetSkillByType(skillType);

            if (assignedLevel == 0)
            {
                skill.Exp = 0;
                skill.Level = 0;
                return;
            }
            
            var expSettings = GameSettings.Instance.ExpSettings;
            var minExp = expSettings.GetMinExpForLevel(assignedLevel);
            skill.Exp = minExp;
            skill.Level = assignedLevel;
        }

        public void AddExpToSkill(ESkillType skillType, float expToAdd, bool includeModifiers = true)
        {
            var skill = GetSkillByType(skillType);

            float moddedExp = expToAdd;

            if (includeModifiers)
            {
                moddedExp += GetExpPassionModifierBonus(skillType, expToAdd);
            }
            
            skill.Exp += moddedExp;

            var expSettings = GameSettings.Instance.ExpSettings;
            skill.Level = expSettings.GetLevelForTotalExp(skill.Exp);
        }

        private float GetExpPassionModifierBonus(ESkillType skillType, float originalAmount)
        {
            var expSettings = GameSettings.Instance.ExpSettings;
            float moddedAmount;
            
            var skillPassion = GetSkillByType(skillType).Passion;
            switch (skillPassion)
            {
                case ESkillPassion.None:
                    moddedAmount = originalAmount * expSettings.NoPassionExpMod;
                    break;
                case ESkillPassion.Minor:
                    moddedAmount = originalAmount * expSettings.MinorPassionExpMod;
                    break;
                case ESkillPassion.Major:
                    moddedAmount = originalAmount * expSettings.MajorPassionExpMod;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return moddedAmount;
        }

        public void DeductExpFromSkill(ESkillType skillType, int expToRemove)
        {
            var skill = GetSkillByType(skillType);

            skill.Exp -= expToRemove;
            
            if (skill.Exp < 0) skill.Exp = 0;
            
            var expSettings = GameSettings.Instance.ExpSettings;
            skill.Level = expSettings.GetLevelForTotalExp(skill.Exp);
        }
        
        public void RandomizeSkillLevels()
        {
            var expSettings = GameSettings.Instance.ExpSettings;
            
            var miningExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Mining, miningExp, false);
            MiningSkill.RandomlyAssignPassion();
            
            var cookingExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Cooking, cookingExp, false);
            CookingSkill.RandomlyAssignPassion();
            
            var meleeExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Melee, meleeExp, false);
            MeleeSkill.RandomlyAssignPassion();
            
            var rangedExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Ranged, rangedExp, false);
            RangedSkill.RandomlyAssignPassion();
            
            var constructionExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Construction, constructionExp, false);
            ConstructionSkill.RandomlyAssignPassion();
            
            var botanyExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Botany, botanyExp, false);
            BotanySkill.RandomlyAssignPassion();
            
            var craftingExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Crafting, craftingExp, false);
            CraftingSkill.RandomlyAssignPassion();
            
            var beastMasteryExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.BeastMastery, beastMasteryExp, false);
            BeastMasterySkill.RandomlyAssignPassion();
            
            var medicalExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Medical, medicalExp, false);
            MedicalSkill.RandomlyAssignPassion();
            
            var socialExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Social, socialExp, false);
            SocialSkill.RandomlyAssignPassion();
            
            var intelligenceExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Intelligence, intelligenceExp, false);
            IntelligenceSkill.RandomlyAssignPassion();
        }

        public void DoDailyExpDecay(float decayModifier)
        {
            var expSettings = GameSettings.Instance.ExpSettings;
            
            var miningExp = expSettings.GetDailyDecayRateForLevel(MiningSkill.Level);
            miningExp += (int)Math.Ceiling(miningExp * decayModifier);
            DeductExpFromSkill(ESkillType.Mining, miningExp);
            
            var cookingExp = expSettings.GetDailyDecayRateForLevel(CookingSkill.Level);
            cookingExp += (int)Math.Ceiling(cookingExp * decayModifier);
            DeductExpFromSkill(ESkillType.Cooking, cookingExp);
            
            var meleeExp = expSettings.GetDailyDecayRateForLevel(MeleeSkill.Level);
            meleeExp += (int)Math.Ceiling(meleeExp * decayModifier);
            DeductExpFromSkill(ESkillType.Melee, meleeExp);
            
            var rangedExp = expSettings.GetDailyDecayRateForLevel(RangedSkill.Level);
            rangedExp += (int)Math.Ceiling(rangedExp * decayModifier);
            DeductExpFromSkill(ESkillType.Ranged, rangedExp);
            
            var constructionExp = expSettings.GetDailyDecayRateForLevel(ConstructionSkill.Level);
            constructionExp += (int)Math.Ceiling(constructionExp * decayModifier);
            DeductExpFromSkill(ESkillType.Construction, constructionExp);
            
            var botanyExp = expSettings.GetDailyDecayRateForLevel(BotanySkill.Level);
            botanyExp += (int)Math.Ceiling(botanyExp * decayModifier);
            DeductExpFromSkill(ESkillType.Botany, botanyExp);
            
            var craftingExp = expSettings.GetDailyDecayRateForLevel(CraftingSkill.Level);
            craftingExp += (int)Math.Ceiling(craftingExp * decayModifier);
            DeductExpFromSkill(ESkillType.Crafting, craftingExp);
            
            var beastMasteryExp = expSettings.GetDailyDecayRateForLevel(BeastMasterySkill.Level);
            beastMasteryExp += (int)Math.Ceiling(beastMasteryExp * decayModifier);
            DeductExpFromSkill(ESkillType.BeastMastery, beastMasteryExp);
            
            var medicalExp = expSettings.GetDailyDecayRateForLevel(MedicalSkill.Level);
            medicalExp += (int)Math.Ceiling(medicalExp * decayModifier);
            DeductExpFromSkill(ESkillType.Medical, medicalExp);
            
            var socialExp = expSettings.GetDailyDecayRateForLevel(SocialSkill.Level);
            socialExp += (int)Math.Ceiling(socialExp * decayModifier);
            DeductExpFromSkill(ESkillType.Social, socialExp);
            
            var intelligenceExp = expSettings.GetDailyDecayRateForLevel(IntelligenceSkill.Level);
            intelligenceExp += (int)Math.Ceiling(intelligenceExp * decayModifier);
            DeductExpFromSkill(ESkillType.Intelligence, intelligenceExp);
        }

        public bool CanDoTaskType(ETaskType taskType)
        {
            var associatedSkills = GetAssociatedSkillsForTaskType(taskType);
            foreach (var skill in associatedSkills)
            {
                if (skill.Incapable)
                {
                    return false;
                }
            }

            return true;
        }

        public List<SkillData> GetAssociatedSkillsForTaskType(ETaskType taskType)
        {
            List<SkillData> results = new List<SkillData>();

            switch (taskType)
            {
                case ETaskType.Emergency:
                    break;
                case ETaskType.Healing:
                    results.Add(GetSkillByType(ESkillType.Medical));
                    break;
                case ETaskType.Construction:
                    results.Add(GetSkillByType(ESkillType.Construction));
                    break;
                case ETaskType.AnimalHandling:
                    results.Add(GetSkillByType(ESkillType.BeastMastery));
                    break;
                case ETaskType.Cooking:
                    results.Add(GetSkillByType(ESkillType.Cooking));
                    break;
                case ETaskType.Hunting:
                    results.Add(GetSkillByType(ESkillType.BeastMastery));
                    results.Add(GetSkillByType(ESkillType.Ranged));
                    break;
                case ETaskType.Mining:
                    results.Add(GetSkillByType(ESkillType.Mining));
                    break;
                case ETaskType.Farming:
                case ETaskType.Harvesting:
                case ETaskType.Forestry:
                    results.Add(GetSkillByType(ESkillType.Botany));
                    break;
                case ETaskType.Crafting:
                    results.Add(GetSkillByType(ESkillType.Crafting));
                    break;
                case ETaskType.Hauling:
                    break;
                case ETaskType.Research:
                    results.Add(GetSkillByType(ESkillType.Intelligence));
                    break;
                case ETaskType.Personal:
                    break;
                case ETaskType.Misc:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null);
            }

            return results;
        }
    }
}
