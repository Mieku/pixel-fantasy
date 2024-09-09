using UnityEngine;

namespace DataPersistence
{
    [CreateAssetMenu(fileName = "Fueled Crafting Table Settings", menuName = "Settings/Fueled Crafting Table Settings")]
    public class FueledCraftingTableSettings : CraftingTableSettings, IFueledSettings
    {
        [SerializeField] private FuelSettings _fuelSettings;
        public FuelSettings FuelSettings => _fuelSettings;
    }
}
