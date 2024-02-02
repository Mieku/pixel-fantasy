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
        private Vector2? _eatPos;

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
                    _chair.ClaimSeat(_ai.Kinling);
                    _eatPos = _chair.UseagePosition(_ai.Kinling.transform.position);
                }
            }
            
            _ai.Kinling.KinlingAgent.SetMovePosition(_claimedFood.AssignedStorage.transform.position, () =>
            {
                _claimedFood.AssignedStorage.WithdrawItem(_claimedFood);
                _ai.Kinling.TaskAI.HoldItem(_claimedFood);
                _claimedFood.SetHeld(true);

                if (_chair == null)
                {
                    if (_eateryBuilding == null)
                    {
                        _eatPos = _ai.Kinling.KinlingAgent.PickLocationInRange(5f);
                    }
                    else
                    {
                        _eatPos = _eateryBuilding.GetRandomIndoorsPosition(_ai.Kinling);
                    }
                }
                
                _ai.Kinling.KinlingAgent.SetMovePosition(_eatPos, () =>
                {
                    if (_chair != null)
                    {
                        _chair.EnterSeat(_ai.Kinling);
                        // TODO: Add Sitting and eating Animation
                        _isEating = true;
                        _ai.Kinling.Needs.RegisterNeedChangePerHour(_chair.InUseNeedChange);
                    }
                    else
                    {
                        // TODO: Add Standing and eating Animation
                        _isEating = true;
                    }
                }, OnTaskCancel);
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
                    _ai.Kinling.Needs.IncreaseNeedValue(NeedType.Food, food.FoodNutrition);
                    
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
                    _ai.Kinling.Needs.DeregisterNeedChangePerHour(_chair.InUseNeedChange);
                    _chair.ExitSeat(_ai.Kinling);
                }
                
                _ai.Kinling.kinlingAnimController.SetUnitAction(UnitAction.Nothing);
                
                // If they haven't finished eating, drop the item
                if (_claimedFood != null)
                {
                    _ai.Kinling.TaskAI.DropCarriedItem();
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
