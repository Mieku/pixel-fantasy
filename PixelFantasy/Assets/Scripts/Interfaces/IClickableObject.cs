using System.Collections.Generic;
using Actions;
using Items;

namespace Interfaces
{
    public interface IClickableObject
    {
        public ClickObject GetClickObject();
        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed);
        public List<ActionBase> GetActions();
        public void AssignOrder(ActionBase orderToAssign);
        public bool IsActionActive(ActionBase action);
    }
}
