using Controllers;
using UnityEngine;

namespace Buildings
{
    public class BuildersWorkshop : Building
    {
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }
    }
}
