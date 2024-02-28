using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.SmartObjects.Scripts
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EatAtSeatInteraction : BaseInteraction
    {
        protected class PerformerInfo
        {
            public CommonAIBase Performer;
            public float ElapsedTime;
            public UnityAction<BaseInteraction> OnCompleted;
            public bool HasStarted;
        }

        protected enum EInteractionState
        {
            GoingToStorage,
            GoingToSeat,
        }

        private List<Storage> _storages = new List<Storage>();
        private Building _building;
        private PerformerInfo _currentPerformer;
        private Item _selectedFoodItem;

        private void Start()
        {
            var building = Helper.IsPositionInBuilding(transform.position);
            if (building != null)
            {
                _building = building;
                _building.OnBuildingFurnitureChanged += UpdateBuildingStorages;
                _storages = _building.GetBuildingStorages();
            }
        }
        
        private void UpdateBuildingStorages(List<Furniture> buildingFurniture)
        {
            _storages = _building.GetBuildingStorages();
        }

        public override bool CanPerform(CommonAIBase potentialPerformer) // 1
        {
            if (_currentPerformer != null) return false;
            if (_storages.Count <= 0) return false;

            // Check the storages recorded if there is any food available
            foreach (var storage in _storages)
            {
                if (storage.GetAllFoodItems(false).Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
        
        public override List<InteractionStatChange> GetStatChanges() //2
        {
            var results = base.GetStatChanges();

            // Add in the value of the best food
            List<Item> foodItems = new List<Item>();
            foreach (var storage in _storages)
            {
                var allStorageFood = storage.GetAllFoodItems(false);
                foreach (var foodItem in allStorageFood)
                {
                    foodItems.Add(foodItem);
                }
            }

            // No food available
            if (foodItems.Count == 0)
            {
                Debug.LogError($"Tried to get stat changes but no food was available, this should not happen");
                return null;
            }
            
            List<Item> sortedFoodItems = foodItems.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList();
            _selectedFoodItem = sortedFoodItems[0];
            
            // Make a stat change based on this food item
            var foodData = (IFoodItem)_selectedFoodItem.GetItemData();
            InteractionStatChange foodStat = new InteractionStatChange
            {
                LinkedStat = Librarian.Instance.GetStat("Food"),
                Value = foodData.FoodNutrition
            };

            results.Add(foodStat);
            
            return results;
        }

        public override bool LockInteration(CommonAIBase performer) //3
        {
            if (_currentPerformer != null)
            {
                Debug.LogError($"{performer.name} is trying to lock {_displayName} which is already locked");
                return false;
            }

            // Claim the food in storage
            if (!_selectedFoodItem.AssignedStorage.SetClaimedItem(_selectedFoodItem))
            {
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

        public override bool Perform(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted) //4
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
            
            // Take the item out of storage
            return _currentPerformer.Performer.Kinling.KinlingAgent.SetMovePosition(_selectedFoodItem.AssignedStorage.transform.position, 
                () =>
            {
                _selectedFoodItem.AssignedStorage.WithdrawItem(_selectedFoodItem);
                performer.Kinling.TaskAI.HoldItem(_selectedFoodItem);
                
                _currentPerformer.Performer.Kinling.KinlingAgent.SetMovePosition(gameObject.transform.position, () =>
                {
                    // Start perform animation
                    if (_performingAnimation != UnitAction.Nothing)
                    {
                        performer.Kinling.kinlingAnimController.SetUnitAction(_performingAnimation);
                    }
                    
                    _currentPerformer.ElapsedTime = 0f;
                    _currentPerformer.OnCompleted = onCompleted;
                    _currentPerformer.HasStarted = true;
                });
            });
        }
        
        public override bool UnlockInteraction(CommonAIBase performer) //5
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

            // If food didn't get collected, make sure to un-claim the stored food
            if (_selectedFoodItem != null && _selectedFoodItem.AssignedStorage != null)
            {
                _selectedFoodItem.AssignedStorage.RestoreClaimed(_selectedFoodItem);
            }

            _currentPerformer = null;
            return true;
        }
        
        public override void CancelInteraction(CommonAIBase performer)
        {
            base.OnInteractionCompleted(performer, _currentPerformer.OnCompleted);
            
            performer.Kinling.kinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _currentPerformer.OnCompleted.Invoke(this);
            
            // Drop Item
            if (_selectedFoodItem != null)
            {
                performer.Kinling.TaskAI.DropCarriedItem();
            }
        }
        
        public override void InterruptInteraction()
        {
            if (_currentPerformer != null)
            {
                CancelInteraction(_currentPerformer.Performer);
            }
        }
        
        protected override void OnInteractionCompleted(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted) //6
        {
            base.OnInteractionCompleted(performer, onCompleted);
            
            performer.Kinling.kinlingAnimController.SetUnitAction(UnitAction.Nothing);
            onCompleted.Invoke(this);
            
            Destroy(_selectedFoodItem.gameObject);
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

        /*
         * If outdoors, keep a record of nearby storages
         * If indoors, keep a record of building storages
         */
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(_building != null) return;
            
            // Create a record of nearby storages
            Storage storage = other.GetComponent<Storage>();
            if (storage != null && !_storages.Contains(storage))
            {
                _storages.Add(storage);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(_building != null) return;
            
            var building = Helper.IsPositionInBuilding(other.transform.position);
            if (building != null) return;
            
            // Remove from the record of nearby storages
            Storage storage = other.GetComponent<Storage>();
            if (storage != null && _storages.Contains(storage))
            {
                _storages.Remove(storage);
            }
        }
    }
}
