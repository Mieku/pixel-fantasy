using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
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
    [Serializable]
    public class KinlingData
    {
        public string UniqueID;
        public string Nickname;
        public string Firstname;
        public string Lastname;
        public Vector2 Position;
        [JsonIgnore] public Kinling Kinling;
        public int Age;
        public EGender Gender;
        public string RaceID;
        public ESexualPreference SexualPreference;
        public List<TraitSettings> Traits = new List<TraitSettings>();
        public string PartnerUID;
        public List<string> ChildrenUID = new List<string>();
        public ScheduleData Schedule;
        public bool IsAsleep;
        public FurnitureData AssignedBed;
        public FurnitureData FurnitureInUse;
        public TaskPriorities TaskPriorities;
        public TaskAI.TaskAIState TaskAIState;
        public float WaitingTimer;
        public float IdleTimer;
        [JsonIgnore] public TaskAction CurrentTaskAction; // TODO: For now
        public Item HeldItem;
        public StatsData Stats;
        public NeedsData Needs;
        public MoodData Mood;
        public List<RelationshipData> Relationships = new List<RelationshipData>();
        public AvatarData Avatar;

        [SerializeField]
        private List<LogData> _personalLog = new List<LogData>();

        [JsonIgnore] public RaceSettings Race => GameSettings.Instance.LoadRaceSettings(RaceID);

        [JsonIgnore]
        public KinlingData Partner => KinlingsDatabase.Instance.GetKinlingData(PartnerUID);

        [JsonIgnore]
        public List<KinlingData> Children
        {
            get
            {
                List<KinlingData> results = new List<KinlingData>();
                foreach (var childUID in ChildrenUID)
                {
                    var child = KinlingsDatabase.Instance.GetKinlingData(childUID);
                    results.Add(child);
                }

                return results;
            }
        }

        public void Randomize(RaceSettings race)
        {
            RaceID = race.name;

            if (Helper.RollDice(50))
            {
                Gender = EGender.Male;
            }
            else
            {
                Gender = EGender.Female;
            }
            
            Firstname = Race.GetRandomFirstName(Gender);
            Lastname = Race.GetRandomLastName();
            Nickname = GenerateNickname();
            UniqueID = CreateUID();

            Age = CreateRandomAge(EMaturityStage.Adult);
            SexualPreference = DetermineSexuality();

            Avatar = new AvatarData(this, Gender, MaturityStage, Race);
            Stats = new StatsData();
            Stats.Init(Race);
            Needs = new NeedsData();
            Needs.Initialize(this, Race);
            Mood = new MoodData();
            Mood.Init(this);
            
            AssignHistory(Race.GetRandomHistory());
            AssignTraits(Race.GetRandomTraits(Random.Range(0, 3)));

            TaskPriorities = new TaskPriorities(Stats);
            Schedule = new ScheduleData(); // TODO: Make a nightowl schedule and trait
        }

        public string GenerateNickname()
        {
            if (Helper.RollDice(60))
            {
                if (Helper.RollDice(50))
                {
                    return Firstname;
                }
                else
                {
                    return Lastname;
                }
            }
            else
            {
                return Race.GetRandomNickname(Gender);
            }
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
            Stats.HistoryID = history.name;
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
                    Stats.AddTrait(trait);
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
            Traits = GetTraitsFromParents(mother.Traits, father.Traits);

            Firstname = Race.GetRandomFirstName(Gender);
            Lastname = mother.Lastname;

            Stats.RandomizeSkillLevels();

            UniqueID = CreateUID();
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
            if (Kinling == null || !Kinling.HasInitialized) return;

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

        protected string CreateUID()
        {
            return $"{Fullname}_{Guid.NewGuid()}";
        }
    }
}
