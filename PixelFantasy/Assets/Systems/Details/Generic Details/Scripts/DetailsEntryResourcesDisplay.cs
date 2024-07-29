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

        public void Init(List<CostSettings> resources, string title = "Yields", bool showTilda = true)
        {
            _title.text = $"{title}:";

            foreach (var resource in resources)
            {
                var resourceAmount = Instantiate(_resourceAmountDisplayPrefab, _resourcesLayout);
                resourceAmount.Init(resource, showTilda);
            }
        }
    }
}
