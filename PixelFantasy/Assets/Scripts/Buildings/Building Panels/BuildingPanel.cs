using UnityEngine;
using UnityEngine.Serialization;

namespace Buildings.Building_Panels
{
    public class BuildingPanel : MonoBehaviour
    {
        //[SerializeField] protected WorkerPanel _workerPanel;
        
        [FormerlySerializedAs("_building")] public ProductionBuilding Building;
        protected bool _isInsideBuilding;

        public virtual void Init(Building building)
        {
            Building = building as ProductionBuilding;
        }
        
        public void ChangeView()
        {
            if (_isInsideBuilding)
            {
                _isInsideBuilding = false;
                Building.ViewExterior();
            }
            else
            {
                _isInsideBuilding = true;
                Building.ViewInterior();
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
