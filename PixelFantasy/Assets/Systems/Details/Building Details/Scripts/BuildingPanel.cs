using Buildings;
using UnityEngine;

namespace Systems.Details.Building_Details.Scripts
{
    public abstract class BuildingPanel : MonoBehaviour
    {
        protected Building _building;

        public void Init(Building building)
        {
            _building = building;
            GameEvents.OnBuildingChanged += GameEvent_OnBuildingChanged;
            gameObject.SetActive(true);
            Show();
        }

        public void Hide()
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

        protected abstract void Show();
        protected abstract void Refresh();
    }
}
