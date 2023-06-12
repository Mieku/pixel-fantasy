using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class ProductionPanel : MonoBehaviour
    {
        private ProductionZone _zone;

        public void Show(ProductionZone zone)
        {
            _zone = zone;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
