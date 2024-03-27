// using ScriptableObjects;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Buildings.Building_Panels.Components
// {
//     public class InventoryDisplaySlot : MonoBehaviour
//     {
//         [SerializeField] private Image _itemIcon;
//         [SerializeField] private Image _slotBG;
//         [SerializeField] private TextMeshProUGUI _quantityDisplay;
//         [SerializeField] private Sprite _emptySlotBGSpr, _filledSlotBGSpr;
//
//         public void Clear()
//         {
//             _itemIcon.gameObject.SetActive(false);
//             _quantityDisplay.gameObject.SetActive(false);
//             _slotBG.sprite = _emptySlotBGSpr;
//         }
//
//         public void ShowItem(ItemSettings item, int quantity)
//         {
//             _itemIcon.gameObject.SetActive(true);
//             _itemIcon.sprite = item.ItemSprite;
//             _quantityDisplay.gameObject.SetActive(true);
//             _quantityDisplay.text = quantity.ToString();
//             _slotBG.sprite = _filledSlotBGSpr;
//         }
//     }
// }
