using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public abstract class BuildingPanel : MonoBehaviour
    {
        protected Building _building;
        protected BuildingDetails _buildingDetails;

        public void Init(Building building, BuildingDetails details)
        {
            _building = building;
            _buildingDetails = details;
            GameEvents.OnBuildingChanged += GameEvent_OnBuildingChanged;
            gameObject.SetActive(true);
            Show();
            RefreshLayout();
        }

        public virtual void Hide()
        {
            _building = null;
            GameEvents.OnBuildingChanged -= GameEvent_OnBuildingChanged;
            gameObject.SetActive(false);
        }

        private void GameEvent_OnBuildingChanged(Building changedBuilding)
        {
            if (changedBuilding == _building)
            {
                Refresh();
            }
        }
        
        public void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            _buildingDetails.RefreshLayout();
        }

        protected abstract void Show();
        protected abstract void Refresh();
    }
}
