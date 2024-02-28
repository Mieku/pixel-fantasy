using System;
using System.Collections.Generic;
using Characters;
using Managers;
using TaskSystem;
using UnityEngine;

namespace Systems.CursorHandler.Scripts
{
    public class CommandController : Singleton<CommandController>
    {
        [SerializeField] private CommandControls _controls;
        [SerializeField] private Command _moveCommand;

        private bool IsActiveCommand(Kinling kinling, PlayerInteractable playerInteractable)
        {
            var currentAction = kinling.TaskAI.CurrentAction;
            var pendingCmd = playerInteractable.PendingCommand;

            if (currentAction == null || pendingCmd == null) return false;

            if (currentAction.Task.Requestor == playerInteractable &&
                currentAction.Task == pendingCmd.Task)
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
                        Task task = command.Task;
                        task.SetRequestor(clickObject.Owner.GetPlayerInteractable());
                        task.IsKinlingSpecific = true;
                        task.OnTaskCancel = () =>
                        {
                            task.Requestor.CancelPlayerCommand();
                        };
                        kinlingToHandleCommand.TaskAI.AssignCommandTask(task);
                        clickObject.Owner.GetPlayerInteractable().AssignPlayerCommand(command);
                        HideCommands();
                    }
                } );
        }

        public void ShowMoveCommand(Vector2 worldPos, Kinling kinlingToHandleCommand)
        {
            List<Command> commands = new List<Command> { _moveCommand };
            List<Command> invalidCommands = new List<Command>();
            
            // If the kinling can't move there, make it invalid
            var canPath = kinlingToHandleCommand.KinlingAgent.IsDestinationPossible(worldPos);
            if (!canPath)
            {
                invalidCommands.Add(_moveCommand);
            }
            
            _controls.ShowControls(worldPos, kinlingToHandleCommand.FullName, commands, null, invalidCommands,
                (command) =>
                {
                    Task task = command.Task;
                    task.Payload = worldPos;
                    task.IsKinlingSpecific = true;
                    kinlingToHandleCommand.TaskAI.AssignCommandTask(task);
                    HideCommands();
                } );
        }

        public void HideCommands()
        {
            _controls.HideControls();
        }
    }
}
