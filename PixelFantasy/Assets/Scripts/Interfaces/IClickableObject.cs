using System.Collections.Generic;

namespace Interfaces
{
    public interface IClickableObject
    {
        public ClickObject GetClickObject();
        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed);
        public List<Command> GetCommands();
        public string DisplayName { get; }
        public PlayerInteractable GetPlayerInteractable();
        public void AssignCommand(Command command, object payload = null);
    }

    public interface IClickableTile : IClickableObject
    {
        public void TintTile();
        public void UnTintTile();
    }
}
