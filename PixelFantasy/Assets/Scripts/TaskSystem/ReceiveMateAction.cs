using Characters;
using Items;
using UnityEngine;

namespace TaskSystem
{
    /*
     * Queue the receive mate action with the partner.
     * Walk to the partner and hang out nearby until they also begin their receive mate action.
     * When both are in-sync, continue to walk to each other.
     * When both are nearby, show hearts above their heads.
     * Then they both will walk to their beds and jump in.
     * Once both are in bed, hide the internal view of the house.
     * Animate the house showing hearts and maybe bouncing around.
     * The mate instigator will begin a timer.
     * When the timer completes, provide both members with a mood buff.
     * If the mating was between a male and female, have the female do a pregnancy check.
     * If positive, apply another mood buff for having a child.
     * Spawn a child.
     *
     * If this cancels in any way, inform the partner to also cancel
     */
    public class ReceiveMateAction : TaskAction // ID: Receive Mate
    {
        private Unit _partner;
        private TaskState _taskState;
        private Vector3 _curMovePos;
        private BedFurniture _bed;
        private Vector2 _bedsidePos;
        
        private float _distanceFromPartner => Vector2.Distance(_partner.transform.position, transform.position);

        private enum TaskState
        {
            GoToPartner,
            GoToBed,
            InBed,
            Mating,
        }
        
        public override void PrepareAction(Task task)
        {
            _partner = _ai.Unit.Partner;
            _bed = _ai.Unit.AssignedBed;
            
            _taskState = TaskState.GoToPartner;
        }

        public override void DoAction()
        {
            if (_taskState == TaskState.GoToPartner && _distanceFromPartner > 1.5f)
            {
                if (_curMovePos != _partner.transform.position)
                {
                    _curMovePos = _partner.transform.position;
                    var nearbyPos = Helper.RandomLocationInRange(_curMovePos, 1f);
                    _ai.Unit.UnitAgent.SetMovePosition(nearbyPos);
                }
            } 
            else if (_taskState == TaskState.GoToPartner)
            {
                // Tell partner they are ready
                _ai.Unit.SocialAI.ForceFlirtChatBubble();
                _ai.Unit.SocialAI.ReadyToGoMate = true;
                _taskState = TaskState.GoToBed;
                _bedsidePos = _bed.UseagePosition().position;
                _ai.Unit.UnitAgent.SetMovePosition(_bedsidePos, () =>
                {
                    _taskState = TaskState.InBed;
                    var sleepLocation = _bed.GetSleepLocation(_ai.Unit);
                    // Hop into bed
                    _ai.Unit.UnitAgent.TeleportToPosition(sleepLocation.position, true);
                    _bed.EnterBed(_ai.Unit);
                });
                
            }
            
            if (_taskState == TaskState.InBed)
            {
                // Check if partner is in bed
                if (_ai.Unit.GetUnitState().AssignedHome.InMatingMode)
                {
                    _taskState = TaskState.Mating;
                }
            }

            if (_taskState == TaskState.Mating)
            {
                // Watch household
                if (!_ai.Unit.GetUnitState().AssignedHome.InMatingMode)
                {
                    _ai.Unit.SocialAI.MatingComplete(true);
                    ConcludeAction();
                }
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            _ai.Unit.UnitAgent.TeleportToPosition(_bedsidePos, false);
            _bed.ExitBed(_ai.Unit);
            _ai.Unit.UnitAnimController.SetEyesClosed(false);
            _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            
            ResetValues();
        }
        
        public override void OnTaskCancel()
        {
            base.OnTaskCancel();
            _partner.SocialAI.CancelMateRequest();

            if (_taskState == TaskState.InBed)
            {
                _ai.Unit.UnitAgent.TeleportToPosition(_bedsidePos, false);
                _bed.ExitBed(_ai.Unit);
                _ai.Unit.UnitAnimController.SetEyesClosed(false);
                _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            }
            
            if (_taskState == TaskState.Mating)
            {
                _ai.Unit.SocialAI.MatingComplete(false);
                _ai.Unit.UnitAgent.TeleportToPosition(_bedsidePos, false);
                _bed.ExitBed(_ai.Unit);
                _ai.Unit.UnitAnimController.SetEyesClosed(false);
                _ai.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            }
            
            ResetValues();
        }
        
        private void ResetValues()
        {
            _taskState = TaskState.GoToPartner;
            _bed = null;
        }
    }
}
