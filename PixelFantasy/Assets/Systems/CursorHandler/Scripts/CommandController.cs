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
        
        public void ShowCommands(PlayerInteractable playerInteractable, Kinling kinlingToHandleCommand)
        {
            List<Command> invalidCommands = new List<Command>();
            var allCommands = playerInteractable.GetCommands();
            foreach (var possibleCmd in allCommands)
            {
                if (!possibleCmd.CanDoCommand(kinlingToHandleCommand, playerInteractable))
                {
                    invalidCommands.Add(possibleCmd);
                }
            }
            
            var pendingCmd = playerInteractable.PendingCommand;
            Command inProgressCmd = null;
            
            
            if (pendingCmd != null)
            {
                if (IsActiveCommand(kinlingToHandleCommand, playerInteractable))
                {
                    inProgressCmd = pendingCmd;
                }
                else
                {
                    invalidCommands.Add(pendingCmd);
                }
            }
            
            _controls.ShowControls(playerInteractable.transform, playerInteractable.DisplayName, allCommands, inProgressCmd, invalidCommands,
                (command) =>
                {
                    if (IsActiveCommand(kinlingToHandleCommand, playerInteractable))
                    {
                        // Cancel command
                        playerInteractable.CancelPlayerCommand(command);
                        HideCommands();
                    }
                    else
                    {
                        var requester = playerInteractable;
                        Task task = new Task(command.TaskID, command.DisplayName, command.TaskType, requester);
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
