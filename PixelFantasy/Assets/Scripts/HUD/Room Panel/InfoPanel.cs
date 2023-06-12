using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class InfoPanel : MonoBehaviour
    {
        private RoomZone _zone;

        public void Show(RoomZone zone)
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
