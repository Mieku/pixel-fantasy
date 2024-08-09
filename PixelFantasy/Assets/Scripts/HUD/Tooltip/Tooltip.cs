using System;
using System.Collections.Generic;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD.Tooltip
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _content;
        [SerializeField] private GameObject _headerHandle;
        [SerializeField] private GameObject _lineSeparatorHandle;
        [SerializeField] private GameObject _contentHandle;
        [SerializeField] private GameObject _resourceDisplayHandle;
        [SerializeField] private ResourceCost _resourceCostPrefab;
        [SerializeField] private Transform _resourceCostParent;
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;

        private List<ResourceCost> _displayedCosts = new List<ResourceCost>();

        private void Awake()
        {
            _resourceCostPrefab.gameObject.SetActive(false);
        }

        public void SetText(string content, string header = null, List<CostSettings> costSettingsList = null)
        {
            ClearDisplayedCosts();
            int numElements = 0;
            
            // Handle the Header
            if (string.IsNullOrEmpty(header))
            {
                _headerHandle.SetActive(false);
            }
            else
            {
                numElements++;
                _headerHandle.SetActive(true);
                _header.text = header;
            }

            if (string.IsNullOrEmpty(content))
            {
                _contentHandle.SetActive(false);
            }
            else
            {
                numElements++;
                _contentHandle.SetActive(true);
                _content.text = content;
            }

            // Handle resource costs
            if (costSettingsList is { Count: > 0 })
            {
                numElements++;
                _resourceDisplayHandle.SetActive(true);
                
                foreach (var costSetting in costSettingsList)
                {
                    var costDisplay = Instantiate(_resourceCostPrefab, _resourceCostParent);
                    costDisplay.gameObject.SetActive(true);
                    costDisplay.Init(costSetting);
                    _displayedCosts.Add(costDisplay);
                }
            }
            else
            {
                _resourceDisplayHandle.SetActive(false);
            }

            // Should show the line?
            _lineSeparatorHandle.SetActive(numElements > 1);
        }

        public void OnShow()
        {
            _layoutRebuilder.RefreshLayout();
        }

        public void OnHide()
        {
            _layoutRebuilder.StopRefresh();
        }

        private void ClearDisplayedCosts()
        {
            foreach (var displayedCost in _displayedCosts)
            {
                Destroy(displayedCost.gameObject);
            }
            _displayedCosts.Clear();
        }
        
        private void Update()
        {
            Vector2 pos = Input.mousePosition;
            transform.position = pos;
        }
    }
}
