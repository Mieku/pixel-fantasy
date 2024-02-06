using System;
using System.Collections.Generic;
using Buildings;
using DataPersistence;
using Interfaces;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Mood.Scripts;
using Systems.Social.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Characters
{
    public class Kinling : PlayerInteractable, IClickableObject
    {
        [SerializeField] private RaceData _race;
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private KinlingAppearance _appearance;
        [SerializeField] private Mood _mood;
        [SerializeField] private SocialAI _socialAI;
        
        public Stats Stats;
        
        [Header("Traits")] 
        [SerializeField] protected List<Trait> _traits;
        
        [Header("Income")] 
        [SerializeField] protected int _dailyCoinsIncome;
        
        [SerializeField] private Color _relevantStatColour;
 
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private PositionRendererSorter _positionRendererSorter;
        
        public string FirstName, LastName;
        public Schedule Schedule = new Schedule();

        private HouseholdBuilding _assignedHome;
        public HouseholdBuilding AssignedHome
        {
            get => _assignedHome;
            set
            {
                _assignedHome = value;
                GameEvents.Trigger_OnCoinsIncomeChanged();
            }
        }

        public Building AssignedWorkplace;
        public string JobName => CurrentJob.JobName;

        public string FullName => FirstName + " " + LastName;
        
        public KinlingEquipment Equipment;
        [FormerlySerializedAs("UnitAnimController")] public KinlingAnimController kinlingAnimController;
        public KinlingAgent KinlingAgent;

        public RaceData Race => _race;
        public Mood KinlingMood => _mood;
        public SocialAI SocialAI => _socialAI;
        public List<Trait> AllTraits => _traits;
        public bool IsAsleep;
        public Age Age;
        public EMaturityStage MaturityStage => Age.MaturityStage;
        public ESexualPreference SexualPreference;
        public Gender Gender;
        public Kinling Partner;
        public List<Kinling> Children = new List<Kinling>();
        public ClickObject ClickObject;
        public KinlingNeeds Needs;
        
        private Building _insideBuidling;
        private BedFurniture _bed;
        private KinlingData _kinlingData;
        private ChairFurniture _currentChair;

        private void Awake()
        {
            ClickObject = GetComponent<ClickObject>();
            
            KinlingsManager.Instance.RegisterKinling(this);

            GameEvents.DayTick += GameEvents_DayTick;
        }
        
        private void OnDestroy()
        {
            
            KinlingsManager.Instance.DeregisterKinling(this);
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
            
            GameEvents.DayTick -= GameEvents_DayTick;
        }
        
        public KinlingData GetKinlingData()
        {
            return _kinlingData;
        }

        public void SetKinlingData(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;
            UniqueId = kinlingData.UID;
            FirstName = kinlingData.Firstname;
            LastName = kinlingData.Lastname;
            Gender = kinlingData.Gender;
            SexualPreference = kinlingData.SexualPreference;
            _race = kinlingData.Appearance.Race;
            Age = new Age(kinlingData.CurrentAge, _race.RacialAgeData);
            
            _appearance.Init(this, kinlingData.Appearance);
            Equipment.Init(this, kinlingData.Gear);
            _traits = kinlingData.Traits;

            Stats.Init(kinlingData.Stats);
            
            _mood.Init();

            if (!string.IsNullOrEmpty(kinlingData.Partner))
            {
                Kinling partner = KinlingsManager.Instance.GetUnit(kinlingData.Partner);
                if (partner == null)
                {
                    Debug.LogError($"Can't find Partner: {kinlingData.Partner}");
                    return;
                }

                Partner = partner;
            }

            foreach (var childUID in kinlingData.Children)
            {
                if (!string.IsNullOrEmpty(childUID))
                {
                    Kinling child = KinlingsManager.Instance.GetUnit(childUID);
                    if (child == null)
                    {
                        Debug.LogError($"Can't find child: {childUID}");
                        return;
                    }

                    Children.Add(child);
                }
            }
            
            Initialize();
        }
        
        private void Initialize()
        {
            Needs.Initialize();
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
        }

        public int DailyIncome()
        {
            if (AssignedHome == null)
            {
                return 0;
            }

            return _dailyCoinsIncome;
        }
        
        public void SetInsideBuilding(Building building)
        {
            _insideBuidling = building;
        }

        public void SetSeated(ChairFurniture chair)
        {
            _currentChair = chair;
        }

        public bool IsSeated => _currentChair != null;
        public ChairFurniture GetChair => _currentChair;

        public bool IsIndoors()
        {
            return _insideBuidling != null;
        }

        public void AssignAndLockLayerOrder(int orderInLayer)
        {
            _positionRendererSorter.SetLocked(true);
            _sortingGroup.sortingOrder = orderInLayer;
        }

        public void UnlockLayerOrder()
        {
            _positionRendererSorter.SetLocked(false);
        }

        public TaskAI TaskAI => _taskAI;

        public KinlingAppearance GetAppearance()
        {
            return _appearance;
        }

        public List<NeedTrait> GetStatTraits()
        {
            List<NeedTrait> results = new List<NeedTrait>();
            foreach (var trait in _traits)
            {
                var statTrait = trait as NeedTrait;
                if (statTrait != null)
                {
                    results.Add(statTrait);
                }
            }

            return results;
        }
        
        public MoodThresholdTrait GetMoodThresholdTrait()
        {
            foreach (var trait in _traits)
            {
                var moodThresholdTrait = trait as MoodThresholdTrait;
                if (moodThresholdTrait != null)
                {
                    return moodThresholdTrait;
                }
            }

            return null;
        }
        
        public object CaptureState()
        {
            var appearanceData = _appearance.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                AppearanceData = appearanceData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            _appearance.SetLoadData(unitData.AppearanceData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;
            
            // Unit Appearance
            public KinlingAppearance.AppearanceData AppearanceData;
        }

        public bool IsKinlingAttractedTo(Kinling otherKinling)
        {
            var otherKinlingGender = otherKinling._appearance.GetAppearanceState().Gender;
            switch (SexualPreference)
            {
                case ESexualPreference.None:
                    return false;
                case ESexualPreference.Male:
                    return otherKinlingGender == Gender.Male;
                case ESexualPreference.Female:
                    return otherKinlingGender == Gender.Female;
                case ESexualPreference.Both:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public JobData CurrentJob
        {
            get
            {
                if (AssignedWorkplace == null)
                {
                    return Librarian.Instance.GetJob("Worker");
                }
                else
                {
                    return AssignedWorkplace.GetBuildingJob();
                }
            }
        }
        
        public int RelevantStatScore(List<StatType> relevantStats)
        {
            int score = 0;
            if (relevantStats.Contains(StatType.Strength))
            {
                score += Stats.GetStatByType(StatType.Strength).Level;
            }
            if (relevantStats.Contains(StatType.Vitality))
            {
                score += Stats.GetStatByType(StatType.Vitality).Level;
            }
            if (relevantStats.Contains(StatType.Intelligence))
            {
                score += Stats.GetStatByType(StatType.Intelligence).Level;
            }
            if (relevantStats.Contains(StatType.Expertise))
            {
                score += Stats.GetStatByType(StatType.Expertise).Level;
            }

            return score;
        }
        
        public string GetStatList(List<StatType> relevantStats, Color relevantColourOverride = default)
        {
            Color relevantColour = _relevantStatColour;
            if (relevantColourOverride != default)
            {
                relevantColour = relevantColourOverride;
            }
            
            int strength = Stats.GetStatByType(StatType.Strength).Level;
            int vitality = Stats.GetStatByType(StatType.Vitality).Level;
            int intelligence = Stats.GetStatByType(StatType.Intelligence).Level;
            int expertise = Stats.GetStatByType(StatType.Expertise).Level;

            string result = "";
            // Strength
            if (relevantStats.Contains(StatType.Strength))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{strength} Strength</color><br>";
            }
            else
            {
                result += $"{strength} Strength<br>";
            }
            
            // Vitality
            if (relevantStats.Contains(StatType.Vitality))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{vitality} Vitality</color><br>";
            }
            else
            {
                result += $"{vitality} Vitality<br>";
            }
            
            // Intelligence
            if (relevantStats.Contains(StatType.Intelligence))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{intelligence} Intelligence</color><br>";
            }
            else
            {
                result += $"{intelligence} Intelligence<br>";
            }
            
            // Expertise
            if (relevantStats.Contains(StatType.Expertise))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{expertise} Expertise</color>";
            }
            else
            {
                result += $"{expertise} Expertise";
            }

            return result;
        }

        public void AssignBed(BedFurniture bed)
        {
            _bed = bed;
        }

        public BedFurniture AssignedBed => _bed;

        private void GameEvents_DayTick()
        {
            Age.IncrementAge();
        }

        public ClickObject GetClickObject() => ClickObject;

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }

        public void ToggleAllowed(bool isAllowed)
        {
        }

        public string DisplayName => FullName;
        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public List<Command> GetCommands()
        {
            return new List<Command>(Commands);
        }

        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }

    public enum ESexualPreference
    {
        None,
        Male,
        Female,
        Both,
    }
}
