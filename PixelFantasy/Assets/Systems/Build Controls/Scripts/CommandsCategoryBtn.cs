using System.Collections.Generic;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class CommandsCategoryBtn : CategoryBtn
    {
        [SerializeField] private CommandOptionBtn _commandBtnPrefab;
        [SerializeField] private List<Command> _commandOptions = new List<Command>();

        private List<CommandOptionBtn> _displayedCommands = new List<CommandOptionBtn>();
        
        protected override void DisplayOptions()
        {
            base.DisplayOptions();
            
            foreach (var command in _commandOptions)
            {
                var commandOption = Instantiate(_commandBtnPrefab, _optionsLayout.transform);
                commandOption.Init(command, this);
                _displayedCommands.Add(commandOption);
            }
        }

        protected override void HideOptions()
        {
            base.HideOptions();
            
            foreach (var displayedCommand in _displayedCommands)
            {
                Destroy(displayedCommand.gameObject);
            }
            _displayedCommands.Clear();
        }
    }
}
