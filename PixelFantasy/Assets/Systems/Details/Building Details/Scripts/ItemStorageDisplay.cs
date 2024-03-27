// using ScriptableObjects;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class ItemStorageDisplay : MonoBehaviour
//     {
//         [SerializeField] private TextMeshProUGUI _quantityText;
//         [SerializeField] private Image _itemIcon;
//
//         private ItemSettings _itemSettings;
//         public ItemSettings ItemSettings => _itemSettings;
//
//         public void Init(ItemSettings itemSettings, int amount, int? minimum = null)
//         {
//             _itemSettings = itemSettings;
//             _itemIcon.sprite = _itemSettings.ItemSprite;
//             
//             RefreshValues(amount, minimum);
//         }
//
//         public void RefreshValues(int amount, int? minimum = null)
//         {
//             if (minimum == null)
//             {
//                 _quantityText.text = $"{amount}";
//             }
//             else
//             {
//                 _quantityText.text = $"{amount} / {minimum}";
//             }
//         }
//     }
// }
