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
        public int MeleeLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int MeleeExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int RangedLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int RangedExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int ConstructionLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int ConstructionExp;
        
        [HorizontalLine]

        [ExposeToInspector, DatabrainSerialize]
        public int MiningLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int MiningExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int BotanyLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int BotanyExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int CookingLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int CookingExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int CraftingLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int CraftingExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int BeastMasteryLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int BeastMasteryExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int MedicalLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int MedicalExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int SocialLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int SocialExp;
        
        [HorizontalLine]
        
        [ExposeToInspector, DatabrainSerialize]
        public int IntelligenceLevel;

        [ExposeToInspector, DatabrainSerialize]
        public int IntelligenceExp;
        
        public int GetLevelForSkill(ESkillType skillType)
        {
            switch (skillType)
            {
                case ESkillType.Mining:
                    return MiningLevel;
                case ESkillType.Cooking:
                    return CookingLevel;
                case ESkillType.Melee:
                    return MeleeLevel;
                case ESkillType.Ranged:
                    return RangedLevel;
                case ESkillType.Construction:
                    return ConstructionLevel;
                case ESkillType.Botany:
                    return BotanyLevel;
                case ESkillType.Crafting:
                    return CraftingLevel;
                case ESkillType.BeastMastery:
                    return BeastMasteryLevel;
                case ESkillType.Medical:
                    return MedicalLevel;
                case ESkillType.Social:
                    return SocialLevel;
                case ESkillType.Intelligence:
                    return IntelligenceLevel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }

        public void AddExpToSkill(ESkillType skillType, int expToAdd)
        {
            switch (skillType)
            {
                case ESkillType.Mining:
                    MiningExp += expToAdd;
                    break;
                case ESkillType.Cooking:
                    CookingExp += expToAdd;
                    break;
                case ESkillType.Melee:
                    MeleeExp += expToAdd;
                    break;
                case ESkillType.Ranged:
                    RangedExp += expToAdd;
                    break;
                case ESkillType.Construction:
                    ConstructionExp += expToAdd;
                    break;
                case ESkillType.Botany:
                    BotanyExp += expToAdd;
                    break;
                case ESkillType.Crafting:
                    CraftingExp += expToAdd;
                    break;
                case ESkillType.BeastMastery:
                    BeastMasteryExp += expToAdd;
                    break;
                case ESkillType.Medical:
                    MedicalExp += expToAdd;
                    break;
                case ESkillType.Social:
                    SocialExp += expToAdd;
                    break;
                case ESkillType.Intelligence:
                    IntelligenceExp += expToAdd;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
        
        public void RandomizeSkillLevels()
        {
            MiningLevel = Random.Range(1, 6);
            CookingLevel = Random.Range(1, 6);
            MeleeLevel = Random.Range(1, 6);
            RangedLevel = Random.Range(1, 6);
            ConstructionLevel = Random.Range(1, 6);
            BotanyLevel = Random.Range(1, 6);
            CraftingLevel = Random.Range(1, 6);
            BeastMasteryLevel = Random.Range(1, 6);
            MedicalLevel = Random.Range(1, 6);
            SocialLevel = Random.Range(1, 6);
            IntelligenceLevel = Random.Range(1, 6);
        }
    }
}
