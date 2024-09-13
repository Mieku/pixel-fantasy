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
        [field: SerializeField] public Transform FlooringParent { get; private set; }
        [field: SerializeField] public Transform MiscParent { get; private set; }

        public void ClearParents()
        {
            ClearChildren(MiscParent);
            ClearChildren(FlooringParent);
            ClearChildren(ResourcesParent);
            ClearChildren(FurnitureParent);
            ClearChildren(StructuresParent);
            ClearChildren(ItemsParent);
            ClearChildren(KinlingsParent);
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
