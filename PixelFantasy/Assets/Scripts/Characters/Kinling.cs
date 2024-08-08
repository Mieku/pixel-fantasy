using System;
using System.Collections.Generic;
using AI;
using Handlers;
using Interfaces;
using Items;
using Managers;
using Systems.Mood.Scripts;
using Systems.Social.Scripts;
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
        public TaskHandler TaskHandler;
        
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private SocialAI _socialAI;
        [SerializeField] private SortingGroup _sortingGroup;
        
        public string FullName => RuntimeData.Firstname + " " + RuntimeData.Lastname;
        public override string UniqueID => RuntimeData.UniqueID;
        
        public override string PendingTaskUID
        {
            get => RuntimeData.CurrentTaskID;
            set => RuntimeData.CurrentTaskID = value;
        }
        
        public KinlingAgent KinlingAgent;
        public Avatar Avatar;
        public Item HeldItem;

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
            if(_isQuitting) return;
            
            KinlingsDatabase.Instance.DeregisterKinling(RuntimeData);
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
            
            GameEvents.DayTick -= GameEvents_DayTick;
            GameEvents.MinuteTick -= GameEvents_MinuteTick;
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
    }

    public enum ESexualPreference
    {
        None,
        Male,
        Female,
        Both,
    }
}
