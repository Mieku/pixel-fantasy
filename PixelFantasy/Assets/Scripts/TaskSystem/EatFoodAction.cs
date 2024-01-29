using Buildings;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class EatFoodAction : TaskAction // Task ID: Eat Food
    {
        private IEateryBuilding _eateryBuilding;
        private Item _claimedFood;
        private ChairFurniture _chair;
        private Vector2 _eatPos;

        private bool _isEating;

        private const float EATING_TIME = 10;
        private float _eatingTimer;
        
        public override bool CanDoTask(Task task)
        {
            _eateryBuilding = task.Requestor as IEateryBuilding;

            // Find Food, starting with eatery, if not then global
            if (_eateryBuilding != null)
            {
                _claimedFood = _eateryBuilding.ClaimBestAvailableFood();
            }

            if (_claimedFood == null)
            {
                _claimedFood = InventoryManager.Instance.FindAndClaimBestAvailableFoodGlobal();
            }

            if (_claimedFood != null)
            {
                return true;
            }
            
            return false;
        }
        
        public override void PrepareAction(Task task)
        {
            // If has a home, find a seat. If no seat, stand some place in home
            if (_eateryBuilding != null)
            {
                _chair = _eateryBuilding.FindAvailableChair();
                
                if (_chair != null)
                {
                    _chair.ClaimSeat(_ai.Unit);
                    _eatPos = _chair.UseagePosition(_ai.Unit.transform.position).position;
                }
            }
            
            _ai.Unit.UnitAgent.SetMovePosition(_claimedFood.AssignedStorage.transform.position, () =>
            {
                _claimedFood.AssignedStorage.WithdrawItem(_claimedFood);
                _ai.Unit.TaskAI.HoldItem(_claimedFood);
                _claimedFood.SetHeld(true);

                if (_chair == null)
                {
                    if (_eateryBuilding == null)
                    {
                        _eatPos = _ai.Unit.UnitAgent.PickLocationInRange(5f);
                    }
                    else
                    {
                        _eatPos = _eateryBuilding.GetRandomIndoorsPosition(_ai.Unit);
                    }
                }
                
                _ai.Unit.UnitAgent.SetMovePosition(_eatPos, () =>
                {
                    if (_chair != null)
                    {
                        _chair.EnterSeat(_ai.Unit);
                        // TODO: Add Sitting and eating Animation
                        _isEating = true;
                        _ai.Unit.Needs.RegisterNeedChangePerHour(_chair.InUseNeedChange);
                    }
                    else
                    {
                        // TODO: Add Standing and eating Animation
                        _isEating = true;
                    }
                });
            });
        }

        public override void DoAction()
        {
            if (_isEating)
            {
                _eatingTimer += TimeManager.Instance.DeltaTime;
                if (_eatingTimer > EATING_TIME)
                {
                    // Apply the Nutrition
                    var food = (IFoodItem)_claimedFood.GetItemData();
                    _ai.Unit.Needs.IncreaseNeedValue(NeedType.Food, food.FoodNutrition);
                    
                    Destroy(_claimedFood.gameObject);
                    _claimedFood = null;
                    ConcludeAction();
                }
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();

            if (_isEating)
            {
                _isEating = false;

                if (_chair != null)
                {
                    _ai.Unit.Needs.DeregisterNeedChangePerHour(_chair.InUseNeedChange);
                    _chair.ExitSeat(_ai.Unit);
                }
                
                _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
                
                // If they haven't finished eating, drop the item
                if (_claimedFood != null)
                {
                    _ai.Unit.TaskAI.DropCarriedItem();
                    _claimedFood.SetHeld(true);
                }
                
                _eateryBuilding = null;
                _claimedFood = null;
                _chair = null;
                _eatPos = default;
                _eatingTimer = 0;
            }
        }
    }
}
