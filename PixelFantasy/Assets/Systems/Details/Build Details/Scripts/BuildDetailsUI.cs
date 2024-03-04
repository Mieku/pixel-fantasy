using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class BuildDetailsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelHandle;
        
        public void Show(string header, List<CraftedItemData> options)
        {
            _panelHandle.SetActive(true);
        }
        
        public void Hide()
        {
            _panelHandle.SetActive(false);
            
        }
    }
}
