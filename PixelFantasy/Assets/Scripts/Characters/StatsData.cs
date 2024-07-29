using System;
using System.Collections.Generic;
using AI;
using Newtonsoft.Json;
using ScriptableObjects;
using Systems.Appearance.Scripts;
using Systems.Stats.Scripts;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class StatsData
    {
        public SkillData MeleeSkill = new SkillData();
        public SkillData RangedSkill = new SkillData();
        public SkillData ConstructionSkill = new SkillData();
        public SkillData MiningSkill = new SkillData();
        public SkillData BotanySkill = new SkillData();
        public SkillData CookingSkill = new SkillData();
        public SkillData CraftingSkill = new SkillData();
        public SkillData BeastMasterySkill = new SkillData();
        public SkillData MedicalSkill = new SkillData();
        public SkillData SocialSkill = new SkillData();
        public SkillData IntelligenceSkill = new SkillData();
        
        public List<string> TraitsIDS = new List<string>();
        public string HistoryID;

        [JsonIgnore] public History History => GameSettings.Instance.LoadHistorySettings(HistoryID);

        [JsonIgnore]
        public IReadOnlyList<Trait> Traits
        {
            get
            {
                List<Trait> results = new List<Trait>();
                foreach (var traitID in TraitsIDS)
                {
                    var trait = GameSettings.Instance.LoadTraitSettings(traitID);
                    results.Add(trait);
                }

                return results.AsReadOnly();
            }
        }
        
        [JsonIgnore] public List<AttributeModifier> AttributeModifiers = new List<AttributeModifier>();

        public void Init(RaceSettings race)
        {
            MeleeSkill.Init(race.GetSkillSettingsByType(ESkillType.Melee));
            RangedSkill.Init(race.GetSkillSettingsByType(ESkillType.Ranged));
            ConstructionSkill.Init(race.GetSkillSettingsByType(ESkillType.Construction));
            MiningSkill.Init(race.GetSkillSettingsByType(ESkillType.Mining));
            BotanySkill.Init(race.GetSkillSettingsByType(ESkillType.Botany));
            CookingSkill.Init(race.GetSkillSettingsByType(ESkillType.Cooking));
            CraftingSkill.Init(race.GetSkillSettingsByType(ESkillType.Crafting));
            BeastMasterySkill.Init(race.GetSkillSettingsByType(ESkillType.BeastMastery));
            MedicalSkill.Init(race.GetSkillSettingsByType(ESkillType.Medical));
            SocialSkill.Init(race.GetSkillSettingsByType(ESkillType.Social));
            IntelligenceSkill.Init(race.GetSkillSettingsByType(ESkillType.Intelligence));
            
            RandomizeSkillLevels();
        }

        public void AddTrait(Trait trait)
        {
            if (TraitsIDS.Contains(trait.name)) return;
            
            TraitsIDS.Add(trait.name);
        }

        public List<SkillData> AllSkills
        {
            get
            {
                List<SkillData> results = new List<SkillData>
                { 
                    MeleeSkill, 
                    RangedSkill, 
                    ConstructionSkill, 
                    MiningSkill, 
                    BotanySkill, 
                    CookingSkill, 
                    CraftingSkill, 
                    BeastMasterySkill, 
                    MedicalSkill, 
                    SocialSkill, 
                    IntelligenceSkill 
                };

                return results;
            }
        }

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

        public bool CheckSkillRequirements(List<SkillRequirement> skillRequirements)
        {
            foreach (var skillRequirement in skillRequirements)
            {
                var skillLvl = GetLevelForSkill(skillRequirement.SkillType);
                if (skillLvl < skillRequirement.MinSkillLevel) return false;
            }

            return true;
        }
        
        public void SetLevelForSkill(ESkillType skillType, int assignedLevel)
        {
            var skill = GetSkillByType(skillType);

            if (assignedLevel == 0)
            {
                skill.TotalExp = 0;
                skill.Level = 0;
                return;
            }
            
            var expSettings = GameSettings.Instance.ExpSettings;
            var minExp = expSettings.GetMinExpForLevel(assignedLevel);
            skill.TotalExp = minExp;
            skill.Level = assignedLevel;
        }

        public void AddExpToSkill(ESkillType skillType, float expToAdd, bool includeModifiers = true)
        {
            var skill = GetSkillByType(skillType);

            float moddedExp = expToAdd;
            
            if (includeModifiers)
            {
                moddedExp += GetAttributeModifierBonus(EAttributeType.LearningModifier, expToAdd);
                moddedExp += GetExpPassionModifierBonus(skillType, expToAdd);
            }
            
            skill.TotalExp += moddedExp;

            var expSettings = GameSettings.Instance.ExpSettings;
            skill.Level = expSettings.GetLevelForTotalExp(skill.TotalExp);
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

            skill.TotalExp -= expToRemove;
            
            if (skill.TotalExp < 0) skill.TotalExp = 0;
            
            var expSettings = GameSettings.Instance.ExpSettings;
            skill.Level = expSettings.GetLevelForTotalExp(skill.TotalExp);
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
        
        public void DoDailyExpDecay()
        {
            var expSettings = GameSettings.Instance.ExpSettings;
            float decayModifier = GetTotalAttributeModifier(EAttributeType.SkillDecay);
            
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
        
        public float GetSkillAttributeValue(ESkillType skillType, EAttributeType attributeType)
        {
            var settings = GetSkillByType(skillType).Settings;
            if (settings != null)
            {
                return settings.GetValueForLevel(attributeType, GetLevelForSkill(skillType));
            }
            Debug.LogError($"SkillSettings not found for {skillType}");
            return 1;
        }
        
        public float GetActionSpeedForSkill(ESkillType skillType, bool autoAddExp)
        {
            float baseActionWork = GameSettings.Instance.BaseWorkPerAction;
            
            // Modifiers
            float moddedWork = baseActionWork;
            moddedWork += GetAttributeModifierBonus(EAttributeType.GlobalWorkSpeed, baseActionWork);

            if (GameSettings.Instance.FastActions)
            {
                moddedWork += 1000f;
            }

            switch (skillType)
            {
                case ESkillType.Mining:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.MiningSpeed, baseActionWork);
                    break;
                case ESkillType.Cooking:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.CookingSpeed, baseActionWork);
                    break;
                case ESkillType.Construction:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.ConstructionSpeed, baseActionWork);
                    break;
                case ESkillType.Botany:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.BotanySpeed, baseActionWork);
                    break;
                case ESkillType.Crafting:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.CraftingSpeed, baseActionWork);
                    break;
                case ESkillType.BeastMastery:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.BeastWorkSpeed, baseActionWork);
                    break;
                case ESkillType.Medical:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.MedicalSpeed, baseActionWork);
                    break;
                case ESkillType.Intelligence:
                    moddedWork += GetAttributeModifierBonus(EAttributeType.ResearchSpeed, baseActionWork);
                    break;
                case ESkillType.Social:
                case ESkillType.Melee:
                case ESkillType.Ranged:
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }

            if (autoAddExp)
            {
                float expGain = moddedWork;
                expGain *= GameSettings.Instance.ExpSettings.BaseExpPerWork;
                AddExpToSkill(skillType, expGain);
            }
            
            return moddedWork;
        }

        public int DetermineAmountYielded(ESkillType skillType, int dropAmount)
        {
            float moddedYield = dropAmount;

            switch (skillType)
            {
                case ESkillType.Mining:
                    moddedYield += GetAttributeModifierBonus(EAttributeType.MiningSpeed, dropAmount);
                    break;
                case ESkillType.Cooking:
                    moddedYield += GetAttributeModifierBonus(EAttributeType.ButcheringYield, dropAmount);
                    break;
                case ESkillType.Botany:
                    moddedYield += GetAttributeModifierBonus(EAttributeType.BotanyYield, dropAmount);
                    break;
                case ESkillType.BeastMastery:
                    moddedYield += GetAttributeModifierBonus(EAttributeType.BeastGatherYield, dropAmount);
                    break;
                case ESkillType.Melee:
                case ESkillType.Ranged:
                case ESkillType.Construction:
                case ESkillType.Crafting:
                case ESkillType.Medical:
                case ESkillType.Social:
                case ESkillType.Intelligence:
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }

            return (int) Math.Ceiling(moddedYield);
        }

        public float GetSocialFrequency()
        {
            float baseSocialFrequency = GameSettings.Instance.BaseSocialFrequency;
            
            // Modifiers
            float moddedAmount = baseSocialFrequency;
            moddedAmount += GetAttributeModifierBonus(EAttributeType.SocialFrequency, baseSocialFrequency);

            return moddedAmount;
        }

        public float GetAttractiveness()
        {
            float baseAttractiveness = 1f;
            
            float moddedAmount = baseAttractiveness;
            moddedAmount += GetAttributeModifierBonus(EAttributeType.Attractiveness, baseAttractiveness);
            
            return moddedAmount;
        }

        public float GetAttributeModifierBonus(EAttributeType attributeType, float originalAmount)
        {
            var totalModifier = GetTotalAttributeModifier(attributeType);
            var result = originalAmount * totalModifier;
            return result;
        }
        
        public float GetTotalAttributeModifier(EAttributeType attributeType)
        {
            float totalModifier = 0;
            var modifiers = AttributeModifiers;
            foreach (var modifier in modifiers)
            {
                if (modifier.AttributeType == attributeType)
                {
                    totalModifier += modifier.Modifier;
                }
            }
            
            switch (attributeType)
            {
                case EAttributeType.GlobalWorkSpeed:
                    break;
                case EAttributeType.WalkSpeed:
                    break;
                case EAttributeType.SkillDecay:
                    break;
                case EAttributeType.Appetite:
                    break;
                case EAttributeType.Attractiveness:
                    break;
                case EAttributeType.SocialFrequency:
                    break;
                case EAttributeType.BeastWorkSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.BeastMastery, EAttributeType.BeastWorkSpeed);
                    break;
                case EAttributeType.BeastGatherYield:
                    totalModifier += GetSkillAttributeValue(ESkillType.BeastMastery, EAttributeType.BeastGatherYield);
                    break;
                case EAttributeType.TameBeastChance:
                    totalModifier += GetSkillAttributeValue(ESkillType.BeastMastery, EAttributeType.TameBeastChance);
                    break;
                case EAttributeType.TrainBeastChance:
                    totalModifier += GetSkillAttributeValue(ESkillType.BeastMastery, EAttributeType.TrainBeastChance);
                    break;
                case EAttributeType.BotanySpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Botany, EAttributeType.BotanySpeed);
                    break;
                case EAttributeType.BotanyYield:
                    totalModifier += GetSkillAttributeValue(ESkillType.Botany, EAttributeType.BotanyYield);
                    break;
                case EAttributeType.ConstructionSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Construction, EAttributeType.ConstructionSpeed);
                    break;
                case EAttributeType.ConstructionSuccessChance:
                    totalModifier += GetSkillAttributeValue(ESkillType.Construction, EAttributeType.ConstructionSuccessChance);
                    break;
                case EAttributeType.CookingSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Cooking, EAttributeType.CookingSpeed);
                    break;
                case EAttributeType.ButcheringYield:
                    totalModifier += GetSkillAttributeValue(ESkillType.Cooking, EAttributeType.ButcheringYield);
                    break;
                case EAttributeType.FoodPoisonChance:
                    totalModifier += GetSkillAttributeValue(ESkillType.Cooking, EAttributeType.FoodPoisonChance);
                    break;
                case EAttributeType.CraftingSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Crafting, EAttributeType.CraftingSpeed);
                    break;
                case EAttributeType.CraftingQuality:
                    totalModifier += GetSkillAttributeValue(ESkillType.Crafting, EAttributeType.CraftingQuality);
                    break;
                case EAttributeType.ResearchSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Intelligence, EAttributeType.ResearchSpeed);
                    break;
                case EAttributeType.LearningModifier:
                    totalModifier += GetSkillAttributeValue(ESkillType.Intelligence, EAttributeType.LearningModifier);
                    break;
                case EAttributeType.MedicalSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Medical, EAttributeType.MedicalSpeed);
                    break;
                case EAttributeType.SurgerySuccessChance:
                    totalModifier += GetSkillAttributeValue(ESkillType.Medical, EAttributeType.SurgerySuccessChance);
                    break;
                case EAttributeType.TendQuality:
                    totalModifier += GetSkillAttributeValue(ESkillType.Medical, EAttributeType.TendQuality);
                    break;
                case EAttributeType.MeleeChanceToHit:
                    totalModifier += GetSkillAttributeValue(ESkillType.Melee, EAttributeType.MeleeChanceToHit);
                    break;
                case EAttributeType.MeleeChanceToDodge:
                    totalModifier += GetSkillAttributeValue(ESkillType.Melee, EAttributeType.MeleeChanceToDodge);
                    break;
                case EAttributeType.MiningSpeed:
                    totalModifier += GetSkillAttributeValue(ESkillType.Mining, EAttributeType.MiningSpeed);
                    break;
                case EAttributeType.MiningYield:
                    totalModifier += GetSkillAttributeValue(ESkillType.Mining, EAttributeType.MiningYield);
                    break;
                case EAttributeType.HuntingStealth:
                    totalModifier += GetSkillAttributeValue(ESkillType.Ranged, EAttributeType.HuntingStealth);
                    break;
                case EAttributeType.RangedAccuracy:
                    totalModifier += GetSkillAttributeValue(ESkillType.Ranged, EAttributeType.RangedAccuracy);
                    break;
                case EAttributeType.TradePriceBuy:
                    totalModifier += GetSkillAttributeValue(ESkillType.Social, EAttributeType.TradePriceBuy);
                    break;
                case EAttributeType.TradePriceSell:
                    totalModifier += GetSkillAttributeValue(ESkillType.Social, EAttributeType.TradePriceSell);
                    break;
                case EAttributeType.SocialImpact:
                    totalModifier += GetSkillAttributeValue(ESkillType.Social, EAttributeType.SocialImpact);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attributeType), attributeType, null);
            }
            
            return totalModifier;
        }
    }
}
