using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class CostDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Color32 _availableColour, _unavailableColour;
        [SerializeField] private Image _itemIcon;

        private Ingredient _ingredient;
        private CostSettings _item;

        public void Init(CostSettings costSettings)
        {
            _item = costSettings;
            _itemIcon.sprite = _item.Item.ItemSprite;
            _amountText.text = costSettings.Quantity + "";

            GameEvents.OnInventoryAvailabilityChanged += CheckAfford;
            
            CheckAfford();
        }

        public void Init(Ingredient ingredient)
        {
            _ingredient = ingredient;
            _itemIcon.sprite = _ingredient.GetIngredientIcon(false);
            _amountText.text = _ingredient.Amount + "";
            
            GameEvents.OnInventoryAvailabilityChanged += CheckAfford;
            
            CheckAfford();
        }

        private void CheckAfford()
        {
            if (_item != null)
            {
                if (_item.CanAfford())
                {
                    _amountText.color = _availableColour;
                }
                else
                {
                    _amountText.color = _unavailableColour;
                }
            }
            else if(_ingredient != null)
            {
                if (_ingredient.CanAfford())
                {
                    _amountText.color = _availableColour;
                }
                else
                {
                    _amountText.color = _unavailableColour;
                }
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= CheckAfford;
        }
    }
}
