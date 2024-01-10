using Characters;
using Items;
using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class GoToSleepAction : TaskAction
    {
        private BedFurniture _bed;
        private Vector2 _bedsidePos;
        private bool _isAsleep;

        private const float NO_BED_ENERGY_PER_HOUR = 0.10f;
        
        public override void PrepareAction(Task task)
        {
            _bed = _ai.Unit.AssignedBed;
            
            if (_bed != null)
            {
                // Walk to the bed
                _bedsidePos = _bed.UseagePosition().position;
                var sleepLocation = _bed.GetSleepLocation(_ai.Unit);
                _ai.Unit.UnitAgent.SetMovePosition(_bedsidePos, () =>
                {
                    // Hop into bed
                    _ai.Unit.UnitAgent.TeleportToPosition(sleepLocation.position, true);
                    _bed.EnterBed(_ai.Unit);
                    _ai.Unit.UnitAnimController.SetEyesClosed(true);
                    _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Sleeping);
                    _isAsleep = true;
                    _ai.Unit.Stats.RegisterStatChangePerHour(_bed.IsUseStatChange);
                });
            }
            else
            {
                // Bedless...
                
                // Sleep on floor
                _ai.Unit.UnitAnimController.SetEyesClosed(true);
                _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Sleeping);
                _isAsleep = true;
                _ai.Unit.Stats.RegisterStatChangePerHour("sleep on ground", StatType.Energy, NO_BED_ENERGY_PER_HOUR);
            }
        }

        public override void DoAction()
        {
            if (_isAsleep)
            {
                // Check for time to wake up
                if (CheckWakeupTime())
                {
                    ConcludeAction();
                }
            }
        }

        private bool CheckWakeupTime()
        {
            int currentHour = EnvironmentManager.Instance.GameTime.GetCurrentHour24();
            var schedule = _ai.Unit.GetUnitState().Schedule.GetHour(currentHour);
            return schedule != ScheduleOption.Sleep;
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();

            if (_isAsleep)
            {
                _isAsleep = false;
                if (_bed != null)
                {
                    _ai.Unit.Stats.DeregisterStatChangePerHour(_bed.IsUseStatChange);
                    _ai.Unit.UnitAgent.TeleportToPosition(_bedsidePos, false);
                    _bed.ExitBed(_ai.Unit);
                    _ai.Unit.UnitAnimController.SetEyesClosed(false);
                    _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
                }
                else
                {
                    _ai.Unit.Stats.DeregisterStatChangePerHour("sleep on ground");
                    _ai.Unit.UnitAnimController.SetEyesClosed(false);
                    _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
                }
            }
        }

        public override void OnTaskCancel()
        {
            base.OnTaskCancel();
        }
    }
}
