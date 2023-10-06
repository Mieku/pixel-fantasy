using Items;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.SmartObjects.Scripts
{
    public class ItemInteraction : BaseInteraction
    {
        protected class PerformerInfo
        {
            public CommonAIBase Performer;
            public float ElapsedTime;
            public UnityAction<BaseInteraction> OnCompleted;
            public bool HasStarted;
        }
        
        public Item LinkedItem;
        [SerializeField] protected EInteractionType _interactionType = EInteractionType.Instantaneous;
        
        protected bool _isInStorage => LinkedItem.AssignedStorage != null;
        protected PerformerInfo _currentPerformer;
        protected bool _destroyItemAfterInteraction;


        public void Init(Item item, InteractionConfiguration config)
        {
            LinkedItem = item;
            _displayName = config.InteractionName;
            _interactionType = config.InteractionType;
            _statChanges = config.StatChanges;
            _duration = config.Duration;
            _destroyItemAfterInteraction = config.DestroyItemAfterInteraction;
        }
        
        public override bool CanPerform(CommonAIBase potentialPerformer)
        {
            return _currentPerformer == null;
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
            
            if (_isInStorage)
            {
                LinkedItem.AssignedStorage.SetClaimedItemState(LinkedItem);
            }

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
            
            if (_isInStorage) // If it is in storage perform it nearby
            {
                // Take the item out of storage
                LinkedItem.AssignedStorage.WithdrawItem(LinkedItem);
                performer.Unit.TaskAI.HoldItem(LinkedItem);
                LinkedItem.SetHeld(true);
                
                var standPos = _currentPerformer.Performer.Unit.UnitAgent.PickLocationInRange(1.0f);
                return _currentPerformer.Performer.Unit.UnitAgent.SetMovePosition(standPos, () =>
                {
                    if (_interactionType == EInteractionType.Instantaneous)
                    {
                        if (_statChanges.Count > 0)
                        {
                            ApplyStatChanges(performer, 1f);
                        }

                        OnInteractionCompleted(_currentPerformer.Performer, onCompleted);
                    }
                    else if (_interactionType == EInteractionType.OverTime)
                    {
                        // Start perform animation
                        if (_performingAnimation != UnitAction.Nothing)
                        {
                            performer.Unit.UnitAnimController.SetUnitAction(_performingAnimation);
                        }
                        
                        _currentPerformer.ElapsedTime = 0f;
                        _currentPerformer.OnCompleted = onCompleted;
                        _currentPerformer.HasStarted = true;
                    }
                });
            }
            else // If it is not in storage, perform it on location
            {
                // Pickup the item
                var item = transform.parent.GetComponent<Item>();
                performer.Unit.TaskAI.HoldItem(item);
                item.SetHeld(true);
                
                // Check the interaction type
                if (_interactionType == EInteractionType.Instantaneous)
                {
                    if (_statChanges.Count > 0)
                    {
                        ApplyStatChanges(performer, 1f);
                    }

                    OnInteractionCompleted(_currentPerformer.Performer, onCompleted);
                }
                else if (_interactionType == EInteractionType.OverTime)
                {
                    _currentPerformer.ElapsedTime = 0f;
                    _currentPerformer.OnCompleted = onCompleted;
                    _currentPerformer.HasStarted = true;
                }

                return true;
            }

            return false;
        }

        private void Update()
        {
            if (_currentPerformer == null) return;
            if (!_currentPerformer.HasStarted) return;
            
            float previousElaspedTime = _currentPerformer.ElapsedTime;
            _currentPerformer.ElapsedTime = Mathf.Min(_currentPerformer.ElapsedTime + TimeManager.Instance.DeltaTime, _duration);

            if (_statChanges.Count > 0)
            {
                ApplyStatChanges(_currentPerformer.Performer, (_currentPerformer.ElapsedTime - previousElaspedTime) / _duration);
            }
                
            // Interaction Complete?
            if (_currentPerformer.ElapsedTime >= _duration)
            {
                OnInteractionCompleted(_currentPerformer.Performer, _currentPerformer.OnCompleted);
            }
        }
        
        protected void OnInteractionCompleted(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
        {
            performer.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            onCompleted.Invoke(this);

            if (_destroyItemAfterInteraction)
            {
                var item = transform.parent.GetComponent<Item>();
                Destroy(item.gameObject);
            }
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
            
            if (_isInStorage) // if it is still in storage, restore the claim
            {
                LinkedItem.AssignedStorage.RestoreClaimed(LinkedItem);
            }

            _currentPerformer = null;
            return true;
        }
    }
}
