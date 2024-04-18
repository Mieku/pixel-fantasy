using System;
using Databrain.Attributes;
using ScriptableObjects;
using Systems.Stats.Scripts;
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

        public void AddExpToSkill(ESkillType skillType, int expToAdd)
        {
            var skill = GetSkillByType(skillType);
            skill.Exp += expToAdd;

            var expSettings = GameSettings.Instance.ExpSettings;
            skill.Level = expSettings.GetLevelForTotalExp(skill.Exp);
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
            AddExpToSkill(ESkillType.Mining, miningExp);
            
            var cookingExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Cooking, cookingExp);
            
            var meleeExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Melee, meleeExp);
            
            var rangedExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Ranged, rangedExp);
            
            var constructionExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Construction, constructionExp);
            
            var botanyExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Botany, botanyExp);
            
            var craftingExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Crafting, craftingExp);
            
            var beastMasteryExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.BeastMastery, beastMasteryExp);
            
            var medicalExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Medical, medicalExp);
            
            var socialExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Social, socialExp);
            
            var intelligenceExp = expSettings.GetMinExpForLevel(Random.Range(1, 6));
            AddExpToSkill(ESkillType.Intelligence, intelligenceExp);
        }

        public void DoDailyExpDecay()
        {
            var expSettings = GameSettings.Instance.ExpSettings;
            
            var miningExp = expSettings.GetDailyDecayRateForLevel(MiningSkill.Level);
            DeductExpFromSkill(ESkillType.Mining, miningExp);
            
            var cookingExp = expSettings.GetDailyDecayRateForLevel(CookingSkill.Level);
            DeductExpFromSkill(ESkillType.Cooking, cookingExp);
            
            var meleeExp = expSettings.GetDailyDecayRateForLevel(MeleeSkill.Level);
            DeductExpFromSkill(ESkillType.Melee, meleeExp);
            
            var rangedExp = expSettings.GetDailyDecayRateForLevel(RangedSkill.Level);
            DeductExpFromSkill(ESkillType.Ranged, rangedExp);
            
            var constructionExp = expSettings.GetDailyDecayRateForLevel(ConstructionSkill.Level);
            DeductExpFromSkill(ESkillType.Construction, constructionExp);
            
            var botanyExp = expSettings.GetDailyDecayRateForLevel(BotanySkill.Level);
            DeductExpFromSkill(ESkillType.Botany, botanyExp);
            
            var craftingExp = expSettings.GetDailyDecayRateForLevel(CraftingSkill.Level);
            DeductExpFromSkill(ESkillType.Crafting, craftingExp);
            
            var beastMasteryExp = expSettings.GetDailyDecayRateForLevel(BeastMasterySkill.Level);
            DeductExpFromSkill(ESkillType.BeastMastery, beastMasteryExp);
            
            var medicalExp = expSettings.GetDailyDecayRateForLevel(MedicalSkill.Level);
            DeductExpFromSkill(ESkillType.Medical, medicalExp);
            
            var socialExp = expSettings.GetDailyDecayRateForLevel(SocialSkill.Level);
            DeductExpFromSkill(ESkillType.Social, socialExp);
            
            var intelligenceExp = expSettings.GetDailyDecayRateForLevel(IntelligenceSkill.Level);
            DeductExpFromSkill(ESkillType.Intelligence, intelligenceExp);
        }
    }
}
