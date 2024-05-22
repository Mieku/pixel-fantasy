using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IClickableObject
    {
        public ClickObject GetClickObject();
        public bool IsClickDisabled { get; set; }
        public List<Command> GetCommands();
        public string DisplayName { get; }
        public PlayerInteractable GetPlayerInteractable();
        public void AssignCommand(Command command, object payload = null);

        public Action OnChanged { get; set; }
    }

    public interface IClickableTile : IClickableObject
    {
        public void TintTile();
        public void UnTintTile();
    }
}
