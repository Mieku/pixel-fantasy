using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    public class BedFurniture : Furniture
    {
        [SerializeField] private SpriteRenderer _topSheet;

        public void ShowTopSheet(bool isShown)
        {
            _topSheet.gameObject.SetActive(isShown);
            _topSheet.sortingOrder = _spritesRoot.gameObject.GetComponent<SortingGroup>().sortingOrder + 2;
        }

        public int GetBetweenTheSheetsLayerOrder()
        {
            var bedLayer = _spritesRoot.gameObject.GetComponent<SortingGroup>().sortingOrder;
            var sheetLayer = _topSheet.sortingOrder;

            return ((sheetLayer - bedLayer) / 2) + bedLayer;
        }
    }
}
