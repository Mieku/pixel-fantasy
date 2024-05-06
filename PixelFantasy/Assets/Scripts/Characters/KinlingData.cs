using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Items;
using Systems.Appearance.Scripts;
using Systems.Mood.Scripts;
using Systems.Notifications.Scripts;
using Systems.Social.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;
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
        public EGender Gender;

        [ExposeToInspector, DatabrainSerialize]
        public RaceSettings Race;
        
        [ExposeToInspector, DatabrainSerialize] 
        public ESexualPreference SexualPreference;
        
        // [ExposeToInspector, DatabrainSerialize] 
        // public AppearanceData Appearance;

        [ExposeToInspector, DatabrainSerialize] 
        public List<TraitSettings> Traits = new List<TraitSettings>();
        
        [ExposeToInspector, DatabrainSerialize]
        public KinlingData Partner;
        
        [ExposeToInspector, DatabrainSerialize, DataObjectDropdown] 
        public List<KinlingData> Children = new List<KinlingData>();
        
        [FormerlySerializedAs("Schedule")] [ExposeToInspector, DatabrainSerialize] 
        public ScheduleData Schedule;

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
        public StatsData Stats;

        [ExposeToInspector, DatabrainSerialize]
        public NeedsData Needs;

        [ExposeToInspector, DatabrainSerialize]
        public MoodData Mood;
        
        [ExposeToInspector, DatabrainSerialize] 
        public List<RelationshipData> Relationships = new List<RelationshipData>();

        [ExposeToInspector, DatabrainSerialize, SerializeField] 
        private List<LogData> _personalLog = new List<LogData>();

        [ExposeToInspector, DatabrainSerialize]
        public AvatarData Avatar;

        public void Randomize(RaceSettings race)
        {
            Race = race;
            
            if (Helper.RollDice(50))
            {
                Gender = EGender.Male;
            }
            else
            {
                Gender = EGender.Female;
            }

            Age = CreateRandomAge(EMaturityStage.Adult);
            SexualPreference = DetermineSexuality(); 
            
            Avatar = new AvatarData(this, Gender, MaturityStage, Race);
            
            Firstname = Race.GetRandomFirstName(Gender);
            Lastname = Race.GetRandomLastName();
            
            Stats.RandomizeSkillLevels();
            AssignHistory(Race.GetRandomHistory());
            AssignTraits(Race.GetRandomTraits(Random.Range(0, 4)));
        }

        public int CreateRandomAge(EMaturityStage stage)
        {
            switch (stage)
            {
                case EMaturityStage.Child:
                    return Random.Range(0, Race.RacialAgeData.ChildMaxAge);
                case EMaturityStage.Adult:
                    return Random.Range(Race.RacialAgeData.ChildMaxAge, Race.RacialAgeData.AdultMaxAge);
                case EMaturityStage.Senior:
                    return Random.Range(Race.RacialAgeData.AdultMaxAge, Race.RacialAgeData.LifeExpectancy - 3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }
        
        public void AssignHistory(History history)
        {
            Stats.History = history;
            foreach (var modifier in history.Modifiers)
            {
                modifier.ApplyModifier(this);
            }
        }

        public void AssignTraits(List<Trait> traits)
        {
            foreach (var trait in traits)
            {
                if (!Stats.Traits.Contains(trait))
                {
                    Stats.Traits.Add(trait);
                    foreach (var traitModifier in trait.Modifiers)
                    {
                        traitModifier.ApplyModifier(this);
                    }
                }
            }
        }
        
        public MoodThresholdSettings GetMoodThresholdTrait()
        {
            foreach (var trait in Traits)
            {
                var moodThresholdTrait = trait as MoodThresholdSettings;
                if (moodThresholdTrait != null)
                {
                    return moodThresholdTrait;
                }
            }

            return null;
        }
        
        public void InheritData(KinlingData mother, KinlingData father)
        {
            if (Helper.RollDice(50))
            {
                Gender = EGender.Male;
            }
            else
            {
                Gender = EGender.Female;
            }

            SexualPreference = DetermineSexuality();
            //Appearance = new AppearanceData(Gender, mother.Appearance, father.Appearance);
            //Talents = InheritTalentsFromParents(mother.Talents, father.Talents);
            Traits = GetTraitsFromParents(mother.Traits, father.Traits);

            Firstname = Race.GetRandomFirstName(Gender);
            Lastname = mother.Lastname;
            
            Stats.RandomizeSkillLevels();
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
                    if (Gender == EGender.Male)
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
                if (Gender == EGender.Male)
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

        public void MinuteTick()
        {
            if (Kinling == null) return;
            
            Mood.MinuteTick();
            Needs.MinuteTick();
        }

        public void DayTick()
        {
            
            IncrementAge();
            Stats.DoDailyExpDecay();
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
            return Stats.GetLevelForSkill(skillType);
        }

        public void SetLevelForSkill(ESkillType skillType, int assignedLevel)
        {
            Stats.SetLevelForSkill(skillType, assignedLevel);
        }
        
        public bool IsKinlingAttractedTo(KinlingData otherKinling)
        {
            var otherKinlingGender = otherKinling.Gender;
            switch (SexualPreference)
            {
                case ESexualPreference.None:
                    return false;
                case ESexualPreference.Male:
                    return otherKinlingGender == EGender.Male;
                case ESexualPreference.Female:
                    return otherKinlingGender == EGender.Female;
                case ESexualPreference.Both:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SubmitPersonalLog(LogData logData)
        {
            _personalLog.Add(logData);
            GameEvents.Trigger_OnKinlingChanged(this);
        }

        public List<LogData> GetPersonalLog()
        {
            List<LogData> log = _personalLog.ToList();
            return log;
        }
    }
}
