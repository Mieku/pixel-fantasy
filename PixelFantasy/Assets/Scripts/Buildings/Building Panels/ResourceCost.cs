using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ResourceCost : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _quantityText;
        
        public void Init(ItemAmount itemAmount)
        {
            _itemImage.sprite = itemAmount.Item.ItemSprite;
            _quantityText.text = itemAmount.Quantity.ToString();
        }
    }
}
