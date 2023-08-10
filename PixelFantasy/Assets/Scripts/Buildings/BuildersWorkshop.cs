using Controllers;
using UnityEngine;

namespace Buildings
{
    public class BuildersWorkshop : ProductionBuildingOld
    {
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }
    }
}
