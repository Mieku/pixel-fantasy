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
using UnityEngine.Serialization;
using Avatar = Systems.Appearance.Scripts.Avatar;

namespace Characters
{
    public class Kinling : PlayerInteractable
    {
        public KinlingData RuntimeData;
        public TaskHandler TaskHandler;
        public DraftedHandler DraftedHandler;
        
        [SerializeField] private SocialAI _socialAI;
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private EnvironmentDetector _environmentDetector;
        [SerializeField] private WorkDisplay _workDisplay;
        
        public string FullName => RuntimeData.Firstname + " " + RuntimeData.Lastname;
        public override string UniqueID => RuntimeData.UniqueID;
        public override string DisplayName => RuntimeData.Nickname;
        protected override List<SpriteRenderer> SpritesToOutline => new List<SpriteRenderer> { Avatar.Appearance };

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
        [FormerlySerializedAs("HeldItem")] public ItemStack HeldStack;

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
            if (!string.IsNullOrEmpty(data.HeldStackID))
            {
                var stack = ItemsDatabase.Instance.QueryStack(data.HeldStackID);
                var item = ItemsDatabase.Instance.CreateLoadedStack(stack);

                HeldStack = item;
                item.transform.SetParent(transform);
                item.transform.localPosition = Vector3.zero;
            }
            
            // Handles their task
            if (!string.IsNullOrEmpty(data.CurrentTaskID))
            {
                var task = TasksDatabase.Instance.QueryTask(data.CurrentTaskID);
                TaskHandler.LoadCurrentTask(task);
            }
            
            RefreshDraftCommands();
            
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

        private void Update()
        {
            if (HeldStack != null)
            {
                HeldStack.UpdatePosition();
            }
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
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
        
        public ItemStack HoldItem(ItemStack itemStack, string itemDataUID)
        {
            ItemStack pickedUpStack = itemStack.PickUpItem(this, itemDataUID);
            RuntimeData.HeldStackID = pickedUpStack.UniqueID;
            HeldStack = pickedUpStack;
            
            pickedUpStack.transform.SetParent(transform);
            pickedUpStack.transform.localPosition = Vector3.zero;

            return pickedUpStack;
        }
        
        public ItemStack DropCarriedItem()
        {
            if (HeldStack == null) return null;

            HeldStack.transform.SetParent(ParentsManager.Instance.ItemsParent);
            HeldStack.ItemDropped();
            var item = HeldStack;
            HeldStack = null;
            RuntimeData.HeldStackID = null;
            return item;
        }

        public void DepositHeldItemInStorage(IStorage storage)
        {
            if (HeldStack == null) return;
            
            storage.DepositItems(HeldStack);
            var item = HeldStack;
            HeldStack = null;
            
            var heldStackData = ItemsDatabase.Instance.QueryStack(RuntimeData.HeldStackID);
            foreach (var stackedItemDataUID in heldStackData.StackedItemDataUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(stackedItemDataUID);
                itemData.CurrentTaskID = null;
                itemData.CarryingKinlingUID = null;
            }
            
            RuntimeData.HeldStackID = null;
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

        public string GetCurrentTaskDisplay()
        {
            if (IsDrafted)
            {
                return "Drafted";
            }
            else
            {
                return TaskHandler.GetCurrentTaskDisplay();
            }
        }
        
        public bool IsDrafted
        {
            get => RuntimeData.IsDrafted;
            set
            {
                RuntimeData.IsDrafted = value;
                
                RefreshDraftCommands();
                InformChanged();
            }
        }
        
        private void RefreshDraftCommands()
        {
            if (IsDrafted)
            {
                AddCommand("UnDraft", true);
                RemoveCommand("Draft");
            }
            else
            {
                AddCommand("Draft", true);
                RemoveCommand("UnDraft");
            }
        }

        public override void AssignCommand(Command command)
        {
            if (command.CommandID == "Draft")
            {
                // Return all active and queued tasks
                TaskHandler.StopAndUnclaimTasks();
                
                IsDrafted = true;
                DraftedHandler.SetDrafted(true);
            }
            else if (command.CommandID == "UnDraft")
            {
                IsDrafted = false;
                DraftedHandler.SetDrafted(false);
            }
            else
            {
                base.AssignCommand(command);
            }
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
