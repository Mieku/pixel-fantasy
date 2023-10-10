using System;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class EquipItemAction : TaskAction
    {
        private GearState _claimedGear;
        private GearData _gear;
        private TaskState _state;

        public float DistanceToStorage => Vector2.Distance(_claimedGear.Storage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            var request = task.Materials[0];
            _claimedGear = request.Item.State as GearState;
            _gear = _claimedGear.GearData;
            _state = TaskState.GoingToStorage;
            _ai.Unit.UnitAgent.SetMovePosition(_claimedGear.Storage.transform.position);
        }

        public override void DoAction()
        {
            switch (_state)
            {
                case TaskState.GoingToStorage:
                    DoGoingToStorage();
                    break;
                case TaskState.UnequipCurrentGear:
                    DoUnequipCurrentGear();
                    break;
                case TaskState.EquipNewGear:
                    DoEquipNewGear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoGoingToStorage()
        {
            if (DistanceToStorage <= 1f)
            {
                _state = TaskState.UnequipCurrentGear;
            }
        }

        private void DoUnequipCurrentGear()
        {
            var curEquippedItem = _ai.Unit.Equipment.EquipmentState.GetGearByType(_gear.Type);
            if (curEquippedItem != null)
            {
                _ai.Unit.Equipment.Unequip(curEquippedItem);
            }

            _state = TaskState.EquipNewGear;
        }

        private void DoEquipNewGear()
        {
            var item = _claimedGear.Storage.WithdrawItem(_claimedGear);
            _ai.Unit.Equipment.Equip(item);
            ConcludeAction();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            _task = null;
            _state = TaskState.GoingToStorage;
            _claimedGear = null;
            _gear = null;
        }
        
        public enum TaskState
        {
            GoingToStorage,
            UnequipCurrentGear,
            EquipNewGear,
        }
    }
}
