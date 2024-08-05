using System;
using System.Collections.Generic;
using Characters;
using Managers;
using UnityEngine;

namespace Systems.Kinling_Selector.Scripts
{
    public class KinlingSelector : MonoBehaviour
    {
        [SerializeField] private KinlingDisplay _kinlingDisplayPrefab;

        public static KinlingSelector Instance;
        private readonly List<KinlingDisplay> _displayedKinlings = new List<KinlingDisplay>();

        protected void Awake()
        {
            Instance = this;
            _kinlingDisplayPrefab.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddKinling(KinlingData kinlingData)
        {
            var displayedKinling = _displayedKinlings.Find(display => display.KinlingData == kinlingData);
            if (displayedKinling != null)
            {
                Debug.LogError("Attempted to add an already existing kinling");
                return;
            }

            var display = Instantiate(_kinlingDisplayPrefab, transform);
            display.gameObject.SetActive(true);
            display.Init(kinlingData);
            _displayedKinlings.Add(display);
        }

        public void RemoveKinling(KinlingData kinlingData)
        {
            var displayedKinling = _displayedKinlings.Find(display => display.KinlingData == kinlingData);
            if (displayedKinling != null)
            {
                _displayedKinlings.Remove(displayedKinling);
                Destroy(displayedKinling.gameObject);
            }
        }
    }
}
