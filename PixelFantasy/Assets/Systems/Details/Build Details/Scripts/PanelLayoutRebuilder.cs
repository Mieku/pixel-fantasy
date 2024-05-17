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
            
            var allLayouts = transform.GetComponentsInChildren<RectTransform>().ToList();
            var localRect = transform.GetComponent<RectTransform>();
            if (localRect != null)
            {
                allLayouts.Add(localRect);
            }

            foreach (var layoutRect in allLayouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            }
        }
    }
}
