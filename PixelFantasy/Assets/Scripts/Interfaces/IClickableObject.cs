using System.Collections.Generic;
using Items;

namespace Interfaces
{
    public interface IClickableObject
    {
        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed);
        

        public List<Order> GetOrders();
        public bool IsOrderActive(Order order);
    }
}
