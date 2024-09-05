using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;

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
        
        [BoxGroup("Pivots"), SerializeField] private Vector2 _bottomLeftPivot;
        [BoxGroup("Pivots"), SerializeField] private Vector2 _bottomRightPivot;
        [BoxGroup("Pivots"), SerializeField] private Vector2 _topLeftPivot;
        [BoxGroup("Pivots"), SerializeField] private Vector2 _topRightPivot;

        private List<ResourceCost> _displayedCosts = new List<ResourceCost>();
        private RectTransform _tooltipRectTransform;

        private void Awake()
        {
            _resourceCostPrefab.gameObject.SetActive(false);
            _tooltipRectTransform = GetComponent<RectTransform>();
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
            Vector2 mousePosition = Input.mousePosition;
            Vector2 tooltipPosition = mousePosition;

            // Default pivot is bottom-left with the offset of (-0.1, 0)
            Vector2 newPivot = _bottomLeftPivot;

            // Get screen width and height
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Get tooltip's size
            Vector2 tooltipSize = _tooltipRectTransform.sizeDelta;

            // Adjust pivot based on proximity to screen edges

            // Check if too close to the right edge
            if (mousePosition.x + tooltipSize.x > screenSize.x)
            {
                newPivot = _bottomRightPivot;
            }

            // Check if too close to the top edge
            if (mousePosition.y + tooltipSize.y > screenSize.y)
            {
                if (Mathf.Approximately(newPivot.x, _bottomRightPivot.x)) // Top-right
                {
                    newPivot = _topRightPivot;
                }
                else // Top-left
                {
                    newPivot = _topLeftPivot;
                }
            }

            // Apply the new pivot
            _tooltipRectTransform.pivot = newPivot;

            // Keep the tooltip's position at the mouse's position
            transform.position = tooltipPosition;
        }
    }
}
