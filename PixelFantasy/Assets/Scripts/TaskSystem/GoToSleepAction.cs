// using Characters;
// using Items;
// using Managers;
// using Systems.Appearance.Scripts;
// using UnityEngine;
//
// namespace TaskSystem
// {
//     public class GoToSleepAction : TaskAction
//     {
//         private BedFurniture _bed;
//         private Vector2? _bedsidePos;
//         private bool _isAsleep;
//
//         private const float NO_BED_ENERGY_PER_HOUR = 0.10f;
//         
//         public override void PrepareAction(Task task)
//         {
//             _bed = (BedFurniture)_ai.Kinling.RuntimeData.AssignedBed.GetLinkedFurniture();
//             
//             if (_bed != null)
//             {
//                 // Walk to the bed
//                 _bedsidePos = _bed.UseagePosition(_ai.Kinling.transform.position);
//                 var sleepLocation = _bed.GetSleepLocation(_ai.Kinling);
//                 _ai.Kinling.KinlingAgent.SetMovePosition(_bedsidePos, () =>
//                 {
//                     // Hop into bed
//                     _ai.Kinling.KinlingAgent.TeleportToPosition(sleepLocation.position, true);
//                     _bed.EnterBed(_ai.Kinling);
//                     _ai.Kinling.Avatar.SetEyesClosed(true);
//                     _ai.Kinling.Avatar.SetUnitAction(UnitAction.Sleeping);
//                     _isAsleep = true;
//                     _ai.Kinling.Needs.RegisterNeedChangePerHour(_bed.InUseNeedChange);
//                 }, OnTaskCancel);
//             }
//             else
//             {
//                 // Bedless...
//                 
//                 // Sleep on floor
//                 _ai.Kinling.Avatar.SetEyesClosed(true);
//                 _ai.Kinling.Avatar.SetUnitAction(UnitAction.Sleeping);
//                 _isAsleep = true;
//                 _ai.Kinling.Needs.RegisterNeedChangePerHour("sleep on ground", NeedType.Energy, NO_BED_ENERGY_PER_HOUR);
//             }
//         }
//
//         public override void DoAction()
//         {
//             if (_isAsleep)
//             {
//                 // Check for time to wake up
//                 if (CheckWakeupTime())
//                 {
//                     ConcludeAction();
//                 }
//             }
//         }
//
//         private bool CheckWakeupTime()
//         {
//             int currentHour = EnvironmentManager.Instance.GameTime.GetCurrentHour24();
//             var schedule = _ai.Kinling.RuntimeData.Schedule.GetHour(currentHour);
//             return schedule != ScheduleOption.Sleep;
//         }
//         
//         public override void ConcludeAction()
//         {
//             base.ConcludeAction();
//
//             if (_isAsleep)
//             {
//                 _isAsleep = false;
//                 if (_bed != null)
//                 {
//                     _bedsidePos ??= _bed.transform.position;
//                     
//                     _ai.Kinling.Needs.DeregisterNeedChangePerHour(_bed.InUseNeedChange);
//                     _ai.Kinling.KinlingAgent.TeleportToPosition((Vector2) _bedsidePos, false);
//                     _bed.ExitBed(_ai.Kinling);
//                     _ai.Kinling.Avatar.SetEyesClosed(false);
//                     _ai.Kinling.Avatar.SetUnitAction(UnitAction.Nothing);
//                 }
//                 else
//                 {
//                     _ai.Kinling.Needs.DeregisterNeedChangePerHour("sleep on ground");
//                     _ai.Kinling.Avatar.SetEyesClosed(false);
//                     _ai.Kinling.Avatar.SetUnitAction(UnitAction.Nothing);
//                 }
//             }
//         }
//
//         public override void OnTaskCancel()
//         {
//             base.OnTaskCancel();
//         }
//     }
// }
