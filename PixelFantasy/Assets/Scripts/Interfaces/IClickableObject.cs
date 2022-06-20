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
        public List<Order> GetOrders();// TODO: Get rid of this
        public List<ActionBase> GetActions();
        public void AssignOrder(Order orderToAssign);
        public bool IsOrderActive(Order order);// TODO: Get rid of this
        public bool IsActionActive(ActionBase action);
    }
}
