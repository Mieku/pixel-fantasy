using Handlers;
using UnityEngine.Serialization;

namespace Managers
{
    public class ControllerManager : Singleton<ControllerManager>
    {
        public ItemsHandler ItemsHandler;
        public KinlingsHandler KinlingsHandler;
    }
}
