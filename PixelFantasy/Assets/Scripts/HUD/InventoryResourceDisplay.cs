using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class InventoryResourceDisplay : MonoBehaviour
    {
        [SerializeField] private Image _iconRenderer;
        [SerializeField] private TextMeshProUGUI _amountDisplay;

        public void Init(ItemData itemData, string amount)
        {
            _iconRenderer.sprite = itemData.ItemSprite;
            _amountDisplay.text = amount;
        }

        public void UpdateAmount(string newAmount)
        {
            _amountDisplay.text = newAmount;
        }
    }
}
