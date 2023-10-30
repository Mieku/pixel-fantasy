using Characters;
using Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    public class BedFurniture : Furniture
    {
        public Transform UsingParent;
        
        [SerializeField] private SpriteRenderer _topSheet;

        public void ShowTopSheet(bool isShown)
        {
            _topSheet.gameObject.SetActive(isShown);
            //_topSheet.sortingOrder = _sortingGroup.sortingOrder + 2;
            _topSheet.sortingOrder = 2;
        }

        public void EnterBed(Unit unit)
        {
            unit.transform.SetParent(UsingParent);
            unit.IsAsleep = true;
            AssignFurnitureToKinling(unit);
            ShowTopSheet(true);
            int orderlayer = GetBetweenTheSheetsLayerOrder();
            unit.AssignAndLockLayerOrder(orderlayer);
        }

        public void ExitBed(Unit unit)
        {
            unit.transform.SetParent(UnitsManager.Instance.transform);
            unit.IsAsleep = false;
            ShowTopSheet(false);
            unit.UnlockLayerOrder();
        }

        public int GetBetweenTheSheetsLayerOrder()
        {
            // var bedLayer = _sortingGroup.sortingOrder;
            // var sheetLayer = _topSheet.sortingOrder;
            //
            // return ((sheetLayer - bedLayer) / 2) + bedLayer;
            return 1;
        }
    }
}
