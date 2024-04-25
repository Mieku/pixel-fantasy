using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class PanelLayoutRebuilder : MonoBehaviour
    {
        public void RefreshLayout()
        {
            StartCoroutine(RefreshSequence());
        }
        
        private IEnumerator RefreshSequence() 
        {
            yield return new WaitForEndOfFrame();
            
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
