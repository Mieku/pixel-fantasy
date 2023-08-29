using UnityEngine;

namespace Buildings.Building_Panels
{
    public class ZoningBuildingPanel : MonoBehaviour
    {
        private Building _building;

        public void Init(Building building)
        {
            _building = building;
        }
    }
}
