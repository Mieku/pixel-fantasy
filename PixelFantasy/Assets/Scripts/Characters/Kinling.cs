using System;
using System.Collections.Generic;
using Interfaces;
using Items;
using Managers;
using Systems.Appearance.Scripts;
using Systems.Mood.Scripts;
using Systems.Social.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;
using Avatar = Systems.Appearance.Scripts.Avatar;

namespace Characters
{
    public class Kinling : PlayerInteractable, IClickableObject
    {
        public KinlingData RuntimeData;
        
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private SocialAI _socialAI;
        [SerializeField] private SortingGroup _sortingGroup;
        
        public string FullName => RuntimeData.Firstname + " " + RuntimeData.Lastname;
        
        public KinlingAgent KinlingAgent;
        public Avatar Avatar;

        public StatsData Stats => RuntimeData.Stats;

        public MoodData MoodData => RuntimeData.Mood;
        public NeedsData Needs => RuntimeData.Needs;
        
        public SocialAI SocialAI => _socialAI;
        
        public ClickObject ClickObject;
        public bool HasInitialized { get; private set; }
        public Action OnChanged { get; set; }

        private void Awake()
        {
            ClickObject = GetComponent<ClickObject>();
            
            GameEvents.DayTick += GameEvents_DayTick;
            GameEvents.MinuteTick += GameEvents_MinuteTick;
        }
        
        private void OnDestroy()
        {
            KinlingsDatabase.Instance.DeregisterKinling(RuntimeData);
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
            
            GameEvents.DayTick -= GameEvents_DayTick;
            GameEvents.MinuteTick -= GameEvents_MinuteTick;
        }

        public void SetKinlingData(KinlingData data)
        {
            RuntimeData = data;
            RuntimeData.Kinling = this;

            // if (needsInitialized)
            // {
            //     Initialize();
            // }
            
            HasInitialized = true;
            KinlingsDatabase.Instance.RegisterKinling(RuntimeData);
        }
        
        private void Initialize()
        {
            //Needs.Initialize(this);
            // MoodData.Init(RuntimeData);
            // MoodData.JumpMoodToTarget();
        }

        public void SetSeated(ChairFurniture chair)
        {
            RuntimeData.FurnitureInUse = chair.RuntimeData;
        }

        public bool IsSeated => RuntimeData.FurnitureInUse != null;

        public void AssignAndLockLayerOrder(int orderInLayer)
        {
            _sortingGroup.sortingOrder = orderInLayer;
        }

        public void UnlockLayerOrder()
        {
            //_positionRendererSorter.SetLocked(false);
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
        
        public void AssignBed(BedFurniture bed)
        {
            RuntimeData.AssignedBed = bed.RuntimeData;
        }

        private void GameEvents_DayTick()
        {
            RuntimeData?.DayTick();
        }

        private void GameEvents_MinuteTick()
        {
            RuntimeData?.MinuteTick();
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
