// using System;
// using ScriptableObjects;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Buildings.Building_Panels
// {
//     public class BuildingInventorySlot : MonoBehaviour
//     {
//         [SerializeField] private Image _itemImage;
//         [SerializeField] private TextMeshProUGUI _itemQuantity;
//         [SerializeField] private GameObject _selectedIndicator;
//
//         private ItemData _slotItem;
//         private BuildingLogisticsPanel _logistics;
//         private InventoryLogisticBill _bill;
//         private int _quantity;
//         
//         public ItemData SlotItem => _slotItem;
//         public InventoryLogisticBill Bill => _bill;
//         
//         private void Awake()
//         {
//             _itemImage.gameObject.SetActive(false);
//             _itemQuantity.text = "";
//             _selectedIndicator.SetActive(false);
//         }
//
//         public void Init(BuildingLogisticsPanel logisticsPanel)
//         {
//             _logistics = logisticsPanel;
//         }
//
//         public void Clear()
//         {
//             _slotItem = null;
//             _bill = null;
//             _quantity = 0;
//             
//             Refresh();
//         }
//
//         public void AssignBill(InventoryLogisticBill bill)
//         {
//             _bill = bill;
//             Refresh();
//         }
//
//         public void RemoveBill()
//         {
//             _bill = null;
//             Refresh();
//         }
//
//         public void ShowItem(ItemData item, int quantity)
//         {
//             _slotItem = item;
//             _quantity = quantity;
//             
//             Refresh();
//         }
//
//         private void Refresh()
//         {
//             if (_bill == null)
//             {
//                 if (_slotItem != null)
//                 {
//                     _itemImage.gameObject.SetActive(true);
//                     _itemQuantity.text = _quantity.ToString();
//                     _itemImage.sprite = _slotItem.ItemSprite;
//                 }
//                 else
//                 {
//                     _itemQuantity.text = "";
//                     _itemImage.gameObject.SetActive(false);
//                 }
//             }
//             else
//             {
//                 _itemImage.gameObject.SetActive(true);
//                 _itemQuantity.text = _quantity + _bill.Suffix;
//                 _itemImage.sprite = _bill.Item.ItemSprite;
//             }
//         }
//
//         public void ShowSelector(bool showSelector)
//         {
//             _selectedIndicator.SetActive(showSelector);
//         }
//
//         public void OnSlotPressed()
//         {
//             _logistics.OpenLogistics(this);
//         }
//     }
// }
