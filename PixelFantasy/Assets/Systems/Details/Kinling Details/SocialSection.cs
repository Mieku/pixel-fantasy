using System;
using Characters;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class SocialSection : MonoBehaviour
    {

        private KinlingData _kinlingData;
        private Action _refreshLayoutCallback;

        public void ShowSection(KinlingData kinlingData, Action refreshLayoutCallback)
        {
            _kinlingData = kinlingData;
            _refreshLayoutCallback = refreshLayoutCallback;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _kinlingData = null;
        }
    }
}
