using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class PanelLayoutRebuilder : MonoBehaviour
    {
        public void RefreshLayout()
        {
            var allLayouts = GetComponentsInChildren<RectTransform>().ToList();
            var localRect = GetComponent<RectTransform>();
            if (localRect != null)
            {
                allLayouts.Add(GetComponent<RectTransform>());
            }

            foreach (var layoutRect in allLayouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            }
        }
    }
}
