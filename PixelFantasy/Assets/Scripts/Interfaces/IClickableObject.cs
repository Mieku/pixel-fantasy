using System.Collections.Generic;
using Items;

namespace Interfaces
{
    public interface IClickableObject
    {
        public ClickObject GetClickObject();
        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed);
        public List<Order> GetOrders();
        public void AssignOrder(Order orderToAssign);
        public bool IsOrderActive(Order order);
    }
}
