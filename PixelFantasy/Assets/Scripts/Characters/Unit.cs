using System;
using System.Collections.Generic;
using Buildings;
using DataPersistence;
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
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private RaceData _race;
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private UnitState _unitState;
        [SerializeField] private UnitAppearance _appearance;
        [SerializeField] private Mood _mood;
        [SerializeField] private SocialAI _socialAI;
        
        [Header("Traits")] 
        [SerializeField] protected List<Trait> _traits;
        
        [Header("Income")] 
        [SerializeField] protected int _dailyCoinsIncome;
 
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private PositionRendererSorter _positionRendererSorter;
        
        public KinlingEquipment Equipment;
        public UnitAnimController UnitAnimController;
        public UnitAgent UnitAgent;

        public RaceData Race => _race;
        public Mood KinlingMood => _mood;
        public SocialAI SocialAI => _socialAI;
        public List<Trait> AllTraits => _traits;
        public bool IsAsleep;
        public EMaturityStage MaturityStage;
        public ESexualPreference SexualPreference;
        public Gender Gender;
        public Unit Partner;
        public List<Unit> Children = new List<Unit>();
        public ClickObject ClickObject;
        [FormerlySerializedAs("Stats")] public KinlingNeeds Needs;

        private Building _insideBuidling;
        private BedFurniture _bed;

        private void Awake()
        {
            ClickObject = GetComponent<ClickObject>();
            
            UnitsManager.Instance.RegisterKinling(this);
        }

        private void Start()
        {
            // Equipment.Init(this);
            // _appearance.Init(this);
            // _mood.Init();
            //
            // Family = FamilyManager.Instance.FindOrCreateFamily(this);
            //
            // GameEvents.Trigger_OnCoinsIncomeChanged();
            //
            // Initialize();
        }

        public void LoadKinlingData(KinlingData kinlingData)
        {
            UniqueId = kinlingData.UID;
            _unitState.FirstName = kinlingData.Firstname;
            _unitState.LastName = kinlingData.Lastname;
            MaturityStage = kinlingData.MaturityStage;
            Gender = kinlingData.Gender;
            SexualPreference = kinlingData.SexualPreference;
            _race = kinlingData.Appearance.Race;
            
            _appearance.Init(this, kinlingData.Appearance);
            Equipment.Init(this, kinlingData.Gear);
            _traits = kinlingData.Traits;

            _unitState.Stats = kinlingData.Stats;

            if (!string.IsNullOrEmpty(kinlingData.Partner))
            {
                Unit partner = UnitsManager.Instance.GetUnit(kinlingData.Partner);
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
                    Unit child = UnitsManager.Instance.GetUnit(childUID);
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

        private void OnDestroy()
        {
            
            UnitsManager.Instance.DeregisterKinling(this);
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
        }

        public int DailyIncome()
        {
            if (_unitState.AssignedHome == null)
            {
                return 0;
            }

            return _dailyCoinsIncome;
        }
        
        public void SetInsideBuilding(Building building)
        {
            _insideBuidling = building;
        }

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

        public UnitState GetUnitState()
        {
            return _unitState;
        }

        public TaskAI TaskAI => _taskAI;

        public UnitAppearance GetAppearance()
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
            var unitStateData = _unitState.GetStateData();
            var appearanceData = _appearance.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                UnitStateData = unitStateData,
                AppearanceData = appearanceData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            
            // Send the data to all components
            _unitState.SetLoadData(unitData.UnitStateData);
            _appearance.SetLoadData(unitData.AppearanceData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;

            
            // Unit State
            public UnitState.UnitStateData UnitStateData;
            
            // Unit Appearance
            public UnitAppearance.AppearanceData AppearanceData;
        }

        public bool IsKinlingAttractedTo(Unit otherKinling)
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

        public void AssignBed(BedFurniture bed)
        {
            _bed = bed;
        }

        public BedFurniture AssignedBed => _bed;
    }

    public enum EMaturityStage
    {
        Child = 0,
        Adult = 1,
        Senior = 2,
    }

    public enum ESexualPreference
    {
        None,
        Male,
        Female,
        Both,
    }
}
