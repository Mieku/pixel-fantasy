using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class CardBuildCostDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _costAmount;
        [SerializeField] private Image _costIcon;

        public void Init(ItemAmount itemAmount)
        {
            _costAmount.text = itemAmount.Quantity.ToString();
            _costIcon.sprite = itemAmount.Item.ItemSprite;
        }
    }
}
