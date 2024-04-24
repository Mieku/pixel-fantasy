using Characters;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class NeedsSection : MonoBehaviour
    {
        [SerializeField] private NeedDisplay _foodNeed;
        [SerializeField] private NeedDisplay _energyNeed;
        [SerializeField] private NeedDisplay _funNeed;
        [SerializeField] private NeedDisplay _beautyNeed;
        [SerializeField] private NeedDisplay _comfortNeed;
        
        private KinlingData _kinlingData;

        public void ShowSection(KinlingData kinlingData)
        {
            gameObject.SetActive(true);
            _kinlingData = kinlingData;
            
            _foodNeed.Init(_kinlingData.Kinling.Needs.GetNeedByType(NeedType.Food));
            _energyNeed.Init(_kinlingData.Kinling.Needs.GetNeedByType(NeedType.Energy));
            _funNeed.Init(_kinlingData.Kinling.Needs.GetNeedByType(NeedType.Fun));
            _beautyNeed.Init(_kinlingData.Kinling.Needs.GetNeedByType(NeedType.Beauty));
            _comfortNeed.Init(_kinlingData.Kinling.Needs.GetNeedByType(NeedType.Comfort));

            RefreshNeeds();

            GameEvents.MinuteTick += GameEvent_MinuteTick;
        }
        
        public void Hide()
        {
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
            
            gameObject.SetActive(false);
            _kinlingData = null;
        }

        private void RefreshNeeds()
        {
            _foodNeed.Refresh();
            _energyNeed.Refresh();
            _funNeed.Refresh();
            _beautyNeed.Refresh();
            _comfortNeed.Refresh();
        }

        private void GameEvent_MinuteTick()
        {
            RefreshNeeds();
        }
    }
}
