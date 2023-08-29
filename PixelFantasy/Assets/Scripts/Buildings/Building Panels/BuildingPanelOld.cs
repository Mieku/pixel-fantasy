using UnityEngine;
using UnityEngine.Serialization;

namespace Buildings.Building_Panels
{
    public class BuildingPanelOld : MonoBehaviour
    {
        //[SerializeField] protected WorkerPanel _workerPanel;
        
        [FormerlySerializedAs("Building")] [FormerlySerializedAs("_building")] public ProductionBuildingOld buildingOld;
        protected bool _isInsideBuilding;

        public virtual void Init(BuildingOld buildingOld)
        {
            buildingOld = buildingOld as ProductionBuildingOld;
        }
        
        public void ChangeView()
        {
            if (_isInsideBuilding)
            {
                _isInsideBuilding = false;
                buildingOld.ViewExterior();
            }
            else
            {
                _isInsideBuilding = true;
                buildingOld.ViewInterior();
            }
        }

        public void EnterBuildingPressed()
        {
            ChangeView();
        }

        public void DeconstructBuildingPressed()
        {
            Debug.Log("Not Built Yet!");
        }
    }
}
