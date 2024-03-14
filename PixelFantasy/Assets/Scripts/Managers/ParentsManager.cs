using UnityEngine;

namespace Managers
{
    public class ParentsManager : Singleton<ParentsManager>
    {
        [field: SerializeField] public Transform KinlingsParent { get; private set; }
        [field: SerializeField] public Transform ItemsParent { get; private set; }
        [field: SerializeField] public Transform StructuresParent { get; private set; }
        [field: SerializeField] public Transform FurnitureParent { get; private set; }
        [field: SerializeField] public Transform ResourcesParent { get; private set; }
        [field: SerializeField] public Transform MountainsParent { get; private set; }
    }
}
