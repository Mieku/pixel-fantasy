using Controllers;
using Popups.Zone_Popups;
using UnityEngine;

namespace Buildings
{
    public class Home : Building
    {
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }
    }
}
