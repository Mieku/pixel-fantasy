using System;
using System.Collections.Generic;
using Buildings;
using Databrain;
using Databrain.Attributes;
using DataPersistence;
using Interfaces;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Mood.Scripts;
using Systems.Skills.Scripts;
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
        public DataLibrary DataLibrary;
        
        [DataObjectDropdown("DataLibrary"), SerializeField] 
        private KinlingData _data;
        public KinlingData RuntimeData;
        
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private KinlingAppearance _appearance;
        [SerializeField] private Mood _mood;
        [SerializeField] private SocialAI _socialAI;
        
        public KinlingSkills Skills;
 
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private PositionRendererSorter _positionRendererSorter;
        
        public string FullName => RuntimeData.Firstname + " " + RuntimeData.Lastname;

        public KinlingAnimController kinlingAnimController;
        public KinlingAgent KinlingAgent;


        public Mood KinlingMood => _mood;
        public SocialAI SocialAI => _socialAI;
        
        public ClickObject ClickObject;
        public KinlingNeeds Needs;

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

        public void SetKinlingData(KinlingData data)
        {
            _data = data;
            DataLibrary.RegisterInitializationCallback(() =>
            {
                RuntimeData = (KinlingData) DataLibrary.CloneDataObjectToRuntime(_data, gameObject);
                RuntimeData.Kinling = this;
                
                _appearance.Init(this, _data.Appearance);
                Skills.Init(_data.Talents);
            
                _mood.Init();
            
                Initialize();
                
                // DataLibrary.OnSaved += Saved;
                // DataLibrary.OnLoaded += Loaded;
            });
        }
        
        private void Initialize()
        {
            Needs.Initialize();
        }

        public void SetSeated(ChairFurniture chair)
        {
            RuntimeData.FurnitureInUse = chair.RuntimeData;
        }

        public bool IsSeated => RuntimeData.FurnitureInUse != null;

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

        public List<NeedTraitSettings> GetStatTraits()
        {
            List<NeedTraitSettings> results = new List<NeedTraitSettings>();
            foreach (var trait in RuntimeData.Traits)
            {
                var statTrait = trait as NeedTraitSettings;
                if (statTrait != null)
                {
                    results.Add(statTrait);
                }
            }

            return results;
        }
        
        public MoodThresholdSettings GetMoodThresholdTrait()
        {
            foreach (var trait in RuntimeData.Traits)
            {
                var moodThresholdTrait = trait as MoodThresholdSettings;
                if (moodThresholdTrait != null)
                {
                    return moodThresholdTrait;
                }
            }

            return null;
        }
        
        public bool IsKinlingAttractedTo(Kinling otherKinling)
        {
            var otherKinlingGender = otherKinling._appearance.GetAppearanceState().Gender;
            switch (RuntimeData.SexualPreference)
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
            RuntimeData.AssignedBed = bed.RuntimeData;
        }

        private void GameEvents_DayTick()
        {
            RuntimeData.IncrementAge();
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
