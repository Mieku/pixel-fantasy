using Items;
using UnityEngine;

namespace Characters
{
    public class StagedKinling : MonoBehaviour
    {
        [SerializeField] private KinlingAppearance _appearance;
        //[SerializeField] private KinlingEquipment _equipment;
        
        public void ApplyAppearance(AppearanceState appearanceState)
        {
            AppearanceState clone = new AppearanceState(appearanceState);
            _appearance.ApplyAppearanceState(clone);
        }

        // public void ApplyEquipment(EquipmentState equipmentState)
        // {
        //     EquipmentState clone = new EquipmentState(equipmentState);
        //     _equipment.DisplayEquipmentState(clone);
        // }
    }
}
