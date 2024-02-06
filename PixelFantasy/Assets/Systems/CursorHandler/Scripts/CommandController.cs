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
        
        public void ShowCommands(ClickObject clickObject, Kinling kinlingToHandleCommand)
        {
            List<Command> allowedCommands = new List<Command>();
            var allCommands = clickObject.Owner.GetCommands();
            foreach (var possibleCmd in allCommands)
            {
                if (possibleCmd.CanDoCommand(kinlingToHandleCommand, clickObject.Owner.GetPlayerInteractable()))
                {
                    allowedCommands.Add(possibleCmd);
                }
            }

            var currentAction = kinlingToHandleCommand.TaskAI.CurrentAction;
            var pendingCmd = clickObject.Owner.GetPlayerInteractable().PendingCommand;
            Command inProgressCmd = null;
            List<Command> invalidCommands = new List<Command>();
            
            if (currentAction != null && pendingCmd != null)
            {
                if (currentAction.Task.Requestor == clickObject.Owner.GetPlayerInteractable() && currentAction.Task == pendingCmd.Task)
                {
                    inProgressCmd = pendingCmd;
                }
                else
                {
                    invalidCommands.Add(pendingCmd);
                }
            }
            
            _controls.ShowControls(clickObject.transform, clickObject.Owner.DisplayName, allowedCommands, inProgressCmd, invalidCommands,
                (command) =>
                {
                    Debug.Log($"Command: {command.Name} was selected");
                    Task task = command.Task;
                    task.Requestor = clickObject.Owner.GetPlayerInteractable();
                    kinlingToHandleCommand.TaskAI.AssignCommandTask(task);
                    HideCommands();
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
                    Debug.Log($"Command: {command.Name} was selected");
                    Task task = command.Task;
                    task.Payload = worldPos;
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
