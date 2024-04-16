using UnityEngine;

namespace InfinityPBR.Modules
{
    public interface IFitInInventory
    {
        GameObject GetWorldPrefab();
        GameObject GetInventoryPrefab();
        void SetSpots(int spotY, int spotX);
        int GetSpotInRow();
        int GetSpotInColumn();
        int GetInventoryWidth();
        int GetInventoryHeight();
    }
}