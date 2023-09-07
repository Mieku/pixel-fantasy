using System;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class EquipItemAction : TaskAction
    {
        private Storage _storage;
        private EquipmentData _equipment;
        private TaskState _state;

        public float DistanceToStorage => Vector2.Distance(_storage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            var request = task.Materials[0];
            _storage = request.Storage;
            _equipment = request.ItemData as EquipmentData;
            _state = TaskState.GoingToStorage;
            _ai.Unit.UnitAgent.SetMovePosition(_storage.transform.position);
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
            var curEquippedItem = _ai.Unit.Equipment.GetEquipmentByType(_equipment.Type);
            if (curEquippedItem != null)
            {
                _ai.Unit.Equipment.Unequip(curEquippedItem);
            }

            _state = TaskState.EquipNewGear;
        }

        private void DoEquipNewGear()
        {
            _storage.WithdrawItems(_equipment, 1);
            var item = Spawner.Instance.SpawnItem(_equipment, _storage.transform.position, false);
            _ai.Unit.Equipment.Equip(item);
            ConcludeAction();
        }
        
        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            ConcludeAction();
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            _task = null;
            _state = TaskState.GoingToStorage;
            _storage = null;
            _equipment = null;
        }
        
        public enum TaskState
        {
            GoingToStorage,
            UnequipCurrentGear,
            EquipNewGear,
        }
    }
}
