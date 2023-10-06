using System;
using Characters;
using Items;
using Managers;
using Sirenix.OdinInspector;
using Systems.Traits.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.SmartObjects.Scripts
{
    public class SleepInBedInteraction : BaseInteraction
    {
        protected class PerformerInfo
        {
            public CommonAIBase Performer;
            public float ElapsedTime;
            public UnityAction<BaseInteraction> OnCompleted;
            public bool HasStarted;
        }
        [InfoBox("Note: All Stat Values are per hour!")]
        [SerializeField] private BedFurniture _linkedFurniture;
        [SerializeField] private Transform _sleepLocation;
        
        private PerformerInfo _currentPerformer;
        private Vector2 _preTeleportPos;

        private void Awake()
        {
            GameEvents.MinuteTick += GameEvent_MinuteTick;
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
        }

        public override bool CanPerform(CommonAIBase potentialPerformer)
        {
            // Not currently being used
            if (_currentPerformer != null) return false;
            
            // Is the correct time of day
            if (potentialPerformer.Unit.TaskAI.GetCurrentScheduleOption() != ScheduleOption.Sleep) return false;
            
            // Is the Kinling's home
            if (potentialPerformer.Unit.GetUnitState().AssignedHome != _linkedFurniture.ParentBuilding) return false;

            // Can Kinling use this furniture
            if (!_linkedFurniture.CanKinlingUseThis(potentialPerformer.Unit)) return false;

            return true;
        }

        public override bool LockInteration(CommonAIBase performer)
        {
            if (_currentPerformer != null)
            {
                Debug.LogError($"{performer.name} is trying to lock {_displayName} which is already locked");
                return false;
            }
            
            _currentPerformer = new PerformerInfo()
            {
                Performer = performer,
                ElapsedTime = 0f,
                OnCompleted = null,
                HasStarted = false,
            };

            return true;
        }

        public override bool Perform(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
        {
            if (_currentPerformer == null)
            {
                Debug.LogError($"{performer.name} tried to perform {_displayName} without unlocking");
                return false;
            }

            if (_currentPerformer.Performer != performer)
            {
                Debug.LogError($"{performer.name} tried to perform {_displayName} which was locked by someone else: {_currentPerformer.Performer.name}");
                return false;
            }

            _linkedFurniture.AssignFurnitureToKinling(_currentPerformer.Performer.Unit);
            _linkedFurniture.ShowTopSheet(true);

            int orderlayer = _linkedFurniture.GetBetweenTheSheetsLayerOrder();
            
            // Teleport them into the bed
            _currentPerformer.Performer.Unit.AssignAndLockLayerOrder(orderlayer);
            _preTeleportPos = _currentPerformer.Performer.transform.position;
            _currentPerformer.Performer.Unit.UnitAgent.TeleportToPosition(_sleepLocation.position, true);
            
            // Do sleeping Animation
            performer.Unit.UnitAnimController.SetUnitAction(_performingAnimation);
            
            // Begin timer
            _currentPerformer.ElapsedTime = 0f;
            _currentPerformer.OnCompleted = onCompleted;
            _currentPerformer.HasStarted = true;
            
            return true;
        }

        public override bool UnlockInteraction(CommonAIBase performer)
        {
            if (_currentPerformer == null)
            {
                Debug.LogError($"{performer.name} tried to unlock an already unlocked interaction: {_displayName}");
                return false;
            }

            if (!_currentPerformer.Performer.Equals(performer))
            {
                Debug.LogError($"{performer.name} tried to unlock an interaction that it did not lock: {_displayName}");
                return false;
            }
            
            _currentPerformer = null;
            return true;
        }
        
        protected override void OnInteractionCompleted(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted) //6
        {
            base.OnInteractionCompleted(performer, onCompleted);
            
            _currentPerformer.Performer.Unit.UnitAgent.TeleportToPosition(_preTeleportPos, false);
            _linkedFurniture.ShowTopSheet(true);
            
            _currentPerformer.Performer.Unit.UnlockLayerOrder();

            performer.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            onCompleted.Invoke(this);
        }
        
        private void GameEvent_MinuteTick()
        {
            if (_currentPerformer == null) return;
            if (!_currentPerformer.HasStarted) return;

            // Tick the value, also check if the value is maxed. If so, trigger onComplete
            if (_statChanges.Count > 0)
            {
                ApplyStatChanges(_currentPerformer.Performer, (1f/60f)/100f);
            }

            var curSleepValue = _currentPerformer.Performer.GetStatValue(Librarian.Instance.GetStat("Energy"));
            if (curSleepValue >= 0.99f)
            {
                OnInteractionCompleted(_currentPerformer.Performer, _currentPerformer.OnCompleted);
                return;
            }

            // If no longer sleep time turn it off
            if (_currentPerformer.Performer.Unit.TaskAI.GetCurrentScheduleOption() != ScheduleOption.Sleep)
            {
                OnInteractionCompleted(_currentPerformer.Performer, _currentPerformer.OnCompleted);
            }
        }
    }
}
