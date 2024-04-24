using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Items;
using Systems.Appearance.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    [DataObjectAddToRuntimeLibrary]
    public class KinlingData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize] 
        private string Nickname;
        
        [ExposeToInspector, DatabrainSerialize] 
        public string Firstname;
        
        [ExposeToInspector, DatabrainSerialize] 
        public string Lastname;

        [ExposeToInspector, DatabrainSerialize] 
        public Vector2 Position;

        [ExposeToInspector, DatabrainSerialize]
        public Kinling Kinling;
        
        [ExposeToInspector, DatabrainSerialize] 
        public int Age;
        
        [ExposeToInspector, DatabrainSerialize] 
        public Gender Gender;

        [ExposeToInspector, DatabrainSerialize]
        public RaceSettings Race;
        
        [ExposeToInspector, DatabrainSerialize] 
        public ESexualPreference SexualPreference;
        
        [ExposeToInspector, DatabrainSerialize] 
        public AppearanceData Appearance;

        [ExposeToInspector, DatabrainSerialize] 
        public List<TraitSettings> Traits = new List<TraitSettings>();
        
        [ExposeToInspector, DatabrainSerialize]
        public KinlingData Partner;
        
        [ExposeToInspector, DatabrainSerialize, DataObjectDropdown] 
        public List<KinlingData> Children = new List<KinlingData>();
        
        // [ExposeToInspector, DatabrainSerialize] 
        // public List<TalentSettings> Talents = new List<TalentSettings>();

        [ExposeToInspector, DatabrainSerialize] 
        public Schedule Schedule;

        [ExposeToInspector, DatabrainSerialize]
        public bool IsAsleep;

        [ExposeToInspector, DatabrainSerialize, DataObjectDropdown]
        public FurnitureData AssignedBed;

        [ExposeToInspector, DatabrainSerialize, DataObjectDropdown]
        public FurnitureData FurnitureInUse;

        [ExposeToInspector, DatabrainSerialize]
        public TaskPriorities TaskPriorities;
        
        [ExposeToInspector, DatabrainSerialize]
        public TaskAI.TaskAIState TaskAIState;
        
        [ExposeToInspector, DatabrainSerialize]
        public float WaitingTimer;
        
        [ExposeToInspector, DatabrainSerialize]
        public float IdleTimer;
        
        [ExposeToInspector, DatabrainSerialize]
        public TaskAction CurrentTaskAction;
        
        [ExposeToInspector, DatabrainSerialize]
        public Item HeldItem;

        [ExposeToInspector, DatabrainSerialize]
        public KinlingStatsData StatsData;
        

        public void Randomize(RaceSettings race)
        {
            Race = race;
            
            if (Helper.RollDice(50))
            {
                Gender = Gender.Male;
            }
            else
            {
                Gender = Gender.Female;
            }
            
            SexualPreference = DetermineSexuality(); 
            Appearance = new AppearanceData(race, Gender);
            
            Firstname = Appearance.Race.GetRandomFirstName(Gender);
            Lastname = Appearance.Race.GetRandomLastName();
            
            StatsData.RandomizeSkillLevels();
            AssignHistory(Race.GetRandomHistory());
            AssignTraits(Race.GetRandomTraits(Random.Range(0, 4)));
        }
        
        public void AssignHistory(History history)
        {
            StatsData.History = history;
            foreach (var modifier in history.Modifiers)
            {
                modifier.ApplyModifier(this);
            }
        }

        public void AssignTraits(List<Trait> traits)
        {
            foreach (var trait in traits)
            {
                if (!StatsData.Traits.Contains(trait))
                {
                    StatsData.Traits.Add(trait);
                    foreach (var traitModifier in trait.Modifiers)
                    {
                        traitModifier.ApplyModifier(this);
                    }
                }
            }
        }
        
        public void InheritData(KinlingData mother, KinlingData father)
        {
            if (Helper.RollDice(50))
            {
                Gender = Gender.Male;
            }
            else
            {
                Gender = Gender.Female;
            }

            SexualPreference = DetermineSexuality();
            Appearance = new AppearanceData(Gender, mother.Appearance, father.Appearance);
            //Talents = InheritTalentsFromParents(mother.Talents, father.Talents);
            Traits = GetTraitsFromParents(mother.Traits, father.Traits);

            Firstname = Appearance.Race.GetRandomFirstName(Gender);
            Lastname = mother.Lastname;
            
            StatsData.RandomizeSkillLevels();
        }

        private ESexualPreference DetermineSexuality()
        {
            var sexualityRoll = Random.Range(0f, 100f);
            if (sexualityRoll <= 10f) // According to global statistics roughly 10% of population identifies as not heterosexual
            {
                if (sexualityRoll <= 6f) // According to global statistics roughly 60% of non-heterosexual individuals identify as bi-sexual
                {
                    // Bisexual
                    return ESexualPreference.Both;
                }
                else
                {
                    // Homosexual
                    if (Gender == Gender.Male)
                    {
                        return ESexualPreference.Male;
                    }
                    else
                    {
                        return ESexualPreference.Female;
                    }
                }
            }
            else
            {
                // Heterosexual
                if (Gender == Gender.Male)
                {
                    return ESexualPreference.Female;
                }
                else
                {
                    return ESexualPreference.Male;
                }
            }
        }

        private List<TraitSettings> GetTraitsFromParents(List<TraitSettings> motherTraits, List<TraitSettings> fatherTraits)
        {
            List<TraitSettings> childTraits = new List<TraitSettings>();
            foreach (var motherTrait in motherTraits)
            {
                if (Helper.RollDice(50))
                {
                    childTraits.Add(motherTrait);
                }
            }
            
            foreach (var fatherTrait in fatherTraits)
            {
                if (Helper.RollDice(50))
                {
                    childTraits.Add(fatherTrait);
                }
            }

            childTraits = childTraits.Distinct().ToList();
            return childTraits;
        }

        // private List<TalentSettings> InheritTalentsFromParents(List<TalentSettings> motherTalents, List<TalentSettings> fatherTalents)
        // {
        //     List<TalentSettings> childTalents = new List<TalentSettings>();
        //     foreach (var momTalent in motherTalents)
        //     {
        //         if (Helper.RollDice(50))
        //         {
        //             childTalents.Add(momTalent);
        //         }
        //     }
        //     
        //     foreach (var dadTalent in fatherTalents)
        //     {
        //         if (Helper.RollDice(50))
        //         {
        //             childTalents.Add(dadTalent);
        //         }
        //     }
        //     
        //     childTalents = childTalents.Distinct().ToList();
        //
        //     if (childTalents.Count == 0)
        //     {
        //         if (Helper.RollDice(50))
        //         {
        //             childTalents.Add(Librarian.Instance.GetRandomTalent());
        //         }
        //     }
        //
        //     return childTalents;
        // }
        
        public EMaturityStage MaturityStage
        {
            get
            {
                if (Age <= Race.RacialAgeData.ChildMaxAge)
                {
                    return EMaturityStage.Child;
                }

                if (Age <= Race.RacialAgeData.AdultMaxAge)
                {
                    return EMaturityStage.Adult;
                }

                return EMaturityStage.Senior;
            }
        }

        public int IncrementAge()
        {
            Age++;

            return Age;
        }

        /// <summary>
        /// Returns their nickname if they have one, if not then their firstname
        /// </summary>
        public string GetNickname()
        {
            if (string.IsNullOrEmpty(Nickname))
            {
                return Firstname;
            }

            return Nickname;
        }

        public string Fullname => $"{Firstname} {Lastname}";
        
        public int GetLevelForSkill(ESkillType skillType)
        {
            return StatsData.GetLevelForSkill(skillType);
        }

        public void SetLevelForSkill(ESkillType skillType, int assignedLevel)
        {
            StatsData.SetLevelForSkill(skillType, assignedLevel);
        }

        public float GetTotalAttributeModifier(EAttributeType attributeType, ESkillType? skillType)
        {
            float totalModifier = 0;
            var modifiers = StatsData.AttributeModifiers;
            foreach (var modifier in modifiers)
            {
                if (modifier.AttributeType == attributeType && modifier.AvailableForSkill(skillType))
                {
                    totalModifier += modifier.Modifier;
                }
            }
            
            return totalModifier;
        }
    }
}
