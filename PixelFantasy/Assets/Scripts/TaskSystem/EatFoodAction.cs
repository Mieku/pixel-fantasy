using Buildings;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class EatFoodAction : TaskAction
    {
        private HouseholdBuilding _home;
        private Item _claimedFood;
        private SeatFurniture _seat;
        private Vector2 _eatPos;

        private bool _isEating;

        private const float EATING_TIME = 10;
        private float _eatingTimer;
        
        public override bool CanDoTask(Task task)
        {
            _home = task.Requestor as HouseholdBuilding;

            // Find Food, starting with household, if not then global
            if (_home != null)
            {
                _claimedFood = _home.ClaimBestAvailableFood();
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
            if (_home != null)
            {
                _seat = _home.FindAvailableSeat();
                
                if (_seat != null)
                {
                    _seat.ClaimSeat();
                    _eatPos = _seat.UseagePosition().position;
                }
            }
            
            _ai.Unit.UnitAgent.SetMovePosition(_claimedFood.AssignedStorage.transform.position, () =>
            {
                _claimedFood.AssignedStorage.WithdrawItem(_claimedFood);
                _ai.Unit.TaskAI.HoldItem(_claimedFood);
                _claimedFood.SetHeld(true);

                if (_seat == null)
                {
                    if (_home == null)
                    {
                        _eatPos = _ai.Unit.UnitAgent.PickLocationInRange(5f);
                    }
                    else
                    {
                        _eatPos = _home.GetRandomIndoorsPosition(_ai.Unit);
                    }
                }
                
                _ai.Unit.UnitAgent.SetMovePosition(_eatPos, () =>
                {
                    if (_seat != null)
                    {
                        _seat.EnterSeat(_ai.Unit);
                        // TODO: Add Sitting and eating Animation
                        _isEating = true;
                        _ai.Unit.Needs.RegisterNeedChangePerHour(_seat.InUseNeedChange);
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
                    var food = (RawFoodItemData)_claimedFood.GetItemData();
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

                if (_seat != null)
                {
                    _ai.Unit.Needs.DeregisterNeedChangePerHour(_seat.InUseNeedChange);
                    _seat.ExitSeat(_ai.Unit);
                }
                
                _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
                
                // If they haven't finished eating, drop the item
                if (_claimedFood != null)
                {
                    _ai.Unit.TaskAI.DropCarriedItem();
                    _claimedFood.SetHeld(true);
                }
                
                _home = null;
                _claimedFood = null;
                _seat = null;
                _eatPos = default;
                _eatingTimer = 0;
            }
        }
    }
}
