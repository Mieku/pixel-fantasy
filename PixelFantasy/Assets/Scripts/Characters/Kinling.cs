using System;
using System.Collections.Generic;
using AI;
using Handlers;
using HUD;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Mood.Scripts;
using Systems.Social.Scripts;
using Systems.Traits.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using Avatar = Systems.Appearance.Scripts.Avatar;

namespace Characters
{
    public class Kinling : PlayerInteractable
    {
        public KinlingData RuntimeData;
        public TaskHandler TaskHandler;
        
        [SerializeField] private SocialAI _socialAI;
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private EnvironmentDetector _environmentDetector;
        [SerializeField] private WorkDisplay _workDisplay;
        
        public string FullName => RuntimeData.Firstname + " " + RuntimeData.Lastname;
        public override string UniqueID => RuntimeData.UniqueID;
        public override string DisplayName => RuntimeData.Nickname;

        public override string PendingTaskUID
        {
            get => RuntimeData.CurrentTaskID;
            set => RuntimeData.CurrentTaskID = value;
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            return otherPI is Kinling;
        }

        public KinlingAgent KinlingAgent;
        public Avatar Avatar;
        public Item HeldItem;

        public StatsData Stats => RuntimeData.Stats;

        public MoodData MoodData => RuntimeData.Mood;
        public NeedsData Needs => RuntimeData.Needs;
        
        public SocialAI SocialAI => _socialAI;
        
        public bool HasInitialized { get; private set; }
        
        private void Awake()
        {
            GameEvents.DayTick += GameEvents_DayTick;
            GameEvents.MinuteTick += GameEvents_MinuteTick;
            _environmentDetector.OnVisibilityUpdated += OnLightVisibilityChanged;
            _environmentDetector.OnIsIndoorsUpdated += OnIsIndoorsChanged;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if(_isQuitting) return;
            
            KinlingsDatabase.Instance.DeregisterKinling(RuntimeData);
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
            
            GameEvents.DayTick -= GameEvents_DayTick;
            GameEvents.MinuteTick -= GameEvents_MinuteTick;
            _environmentDetector.OnVisibilityUpdated -= OnLightVisibilityChanged;
            _environmentDetector.OnIsIndoorsUpdated -= OnIsIndoorsChanged;
        }

        public void SetKinlingData(KinlingData data)
        {
            RuntimeData = data;
            Avatar.SetDirection(data.Avatar.CurrentDirection);
            
            KinlingsDatabase.Instance.RegisterKinling(RuntimeData);
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            
            // Handles their held item
            if (!string.IsNullOrEmpty(data.HeldItemID))
            {
                var heldItemData = ItemsDatabase.Instance.Query(data.HeldItemID);
                var item = ItemsDatabase.Instance.CreateItemObject(heldItemData, heldItemData.Position, false);

                HeldItem = item;
                item.transform.SetParent(transform);
                item.transform.localPosition = Vector3.zero;
            }
            
            // Handles their task
            if (!string.IsNullOrEmpty(data.CurrentTaskID))
            {
                var task = TasksDatabase.Instance.QueryTask(data.CurrentTaskID);
                TaskHandler.LoadCurrentTask(task);
            }
            
            HasInitialized = true;
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

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }

        public void ToggleAllowed(bool isAllowed)
        {
        }
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
        
        public void HoldItem(Item item)
        {
            RuntimeData.HeldItemID = item.UniqueID;
            HeldItem = item;
            item.ItemPickedUp(this);
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
        }
        
        public Item DropCarriedItem(bool allowHauling)
        {
            if (HeldItem == null) return null;

            HeldItem.transform.SetParent(ParentsManager.Instance.ItemsParent);
            HeldItem.IsAllowed = allowHauling;
            HeldItem.ItemDropped();
            var item = HeldItem;
            HeldItem = null;
            RuntimeData.HeldItemID = null;
            return item;
        }

        public void DepositHeldItemInStorage(IStorage storage)
        {
            if (HeldItem == null) return;
            
            storage.DepositItems(HeldItem);
            var item = HeldItem;
            HeldItem = null;
            RuntimeData.HeldItemID = null;
            item.RuntimeData.CurrentTaskID = null;
            item.RuntimeData.CarryingKinlingUID = null;
            Destroy(item.gameObject);
        }

        private void OnIsIndoorsChanged(bool isIndoors)
        {
            RuntimeData.IsIndoors = isIndoors;
        }

        private void OnLightVisibilityChanged(float visibility)
        {
            var lowMin = GameSettings.Instance.MinLowVisibility;
            var goodMin = GameSettings.Instance.MinGoodVisibility;
            EVisibilityLevel visibilityLevel;

            if (visibility >= goodMin) // Good
            {
                visibilityLevel = EVisibilityLevel.Good;
            }
            else if(visibility >= lowMin) // Low
            {
                visibilityLevel = EVisibilityLevel.Low;
            }
            else // Blind
            {
                visibilityLevel = EVisibilityLevel.Blind;
            }

            RuntimeData.Stats.VisibilityLevel = visibilityLevel;
        }

        public void DisplayWorkProgress(float percentComplete)
        {
            _workDisplay.SetProgress(percentComplete);
        }

        public void HideWorkProgress()
        {
            _workDisplay.Show(false);
        }

        protected override void OnSelection()
        {
            base.OnSelection();
            KinlingAgent.SetPathVisibility(true);
        }

        protected override void OnDeselection()
        {
            base.OnDeselection();
            KinlingAgent.SetPathVisibility(false);
        }
    }

    public enum ESexualPreference
    {
        None,
        Male,
        Female,
        Both,
    }

    public enum EVisibilityLevel
    {
        Blind,
        Low,
        Good,
    }
}
