using System.Collections.Generic;
using AI;
using Characters;
using Managers;
using UnityEngine;

namespace Systems.CursorHandler.Scripts
{
    public class CommandController : Singleton<CommandController>
    {
        [SerializeField] private CommandControls _controls;
        [SerializeField] private Command _moveCommand;

        private bool IsActiveCommand(Kinling kinling, PlayerInteractable playerInteractable)
        {
            var currentAction = kinling.TaskHandler.CurrentTask;
            var pendingCmd = playerInteractable.PendingCommand;

            if (currentAction == null || pendingCmd == null) return false;

            if (currentAction.RequesterID == playerInteractable.UniqueID &&
                currentAction == pendingCmd.Task)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void ShowCommands(ClickObject clickObject, Kinling kinlingToHandleCommand)
        {
            List<Command> invalidCommands = new List<Command>();
            var allCommands = clickObject.Owner.GetCommands();
            foreach (var possibleCmd in allCommands)
            {
                if (!possibleCmd.CanDoCommand(kinlingToHandleCommand, clickObject.Owner.GetPlayerInteractable()))
                {
                    invalidCommands.Add(possibleCmd);
                }
            }

            var currentAction = kinlingToHandleCommand.TaskAI.CurrentAction;
            var pendingCmd = clickObject.Owner.GetPlayerInteractable().PendingCommand;
            Command inProgressCmd = null;
            
            
            if (currentAction != null && pendingCmd != null)
            {
                if (IsActiveCommand(kinlingToHandleCommand, clickObject.Owner.GetPlayerInteractable()))
                {
                    inProgressCmd = pendingCmd;
                }
                else
                {
                    invalidCommands.Add(pendingCmd);
                }
            }
            
            _controls.ShowControls(clickObject.transform, clickObject.Owner.DisplayName, allCommands, inProgressCmd, invalidCommands,
                (command) =>
                {
                    if (IsActiveCommand(kinlingToHandleCommand, clickObject.Owner.GetPlayerInteractable()))
                    {
                        // Cancel command
                        kinlingToHandleCommand.TaskAI.CancelCurrentTask();
                        clickObject.Owner.GetPlayerInteractable().CancelPlayerCommand(command);
                        HideCommands();
                    }
                    else
                    {
                        var requester = clickObject.Owner.GetPlayerInteractable();
                        Task task = new Task(command.TaskID, command.DisplayName, command.TaskType, requester);
                        task.IsKinlingSpecific = true;
                        task.Status = ETaskStatus.InProgress;
                        TasksDatabase.Instance.AddTask(task);
                        kinlingToHandleCommand.TaskHandler.AssignSpecificTask(task);
                        
                        requester.AssignSpecificTask(task);
                        
                        HideCommands();
                    }
                } );
        }
        
        public void HideCommands()
        {
            _controls.HideControls();
        }
    }
}
