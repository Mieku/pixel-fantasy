using Controllers;
using UnityEngine;

namespace Buildings
{
    public class BuildersWorkshop : ProductionBuilding
    {
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }
    }
}
