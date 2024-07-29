using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class ResourceCost : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _quantityText;

        public ItemSettings ItemSettings { get; private set; }
        private int? _total;

        public void Init(CostSettings costSettings)
        {
            ItemSettings = costSettings.Item;
            _itemImage.sprite = costSettings.Item.ItemSprite;
            _quantityText.text = costSettings.Quantity.ToString();
        }

        public void Init(CostData costSettings, int totalNeeded)
        {
            ItemSettings = costSettings.Item;
            _itemImage.sprite = costSettings.Item.ItemSprite;
            int missingAmount = totalNeeded - costSettings.Quantity;
            _quantityText.text = $"{missingAmount}/{totalNeeded}";
            _total = totalNeeded;
        }

        public void RefreshAmount(CostData costSettings)
        {
            if (_total != null)
            {
                int missingAmount = (int)_total - costSettings.Quantity;
                _quantityText.text = $"{missingAmount}/{_total}";
            }
            else
            {
                _quantityText.text = costSettings.Quantity.ToString();
            }
        }
    }
}
