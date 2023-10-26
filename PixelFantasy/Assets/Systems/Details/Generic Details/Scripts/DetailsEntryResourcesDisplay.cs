using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryResourcesDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Transform _resourcesLayout;
        [SerializeField] private DetailsEntryResourceAmountDisplay _resourceAmountDisplayPrefab;

        public void Init(List<ItemAmount> resources, string title = "Yields")
        {
            _title.text = $"{title}:";

            foreach (var resource in resources)
            {
                var resourceAmount = Instantiate(_resourceAmountDisplayPrefab, _resourcesLayout);
                resourceAmount.Init(resource);
            }
        }
    }
}
