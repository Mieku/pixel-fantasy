using HUD.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftQueueItem : MonoBehaviour
    {
        [SerializeField] private TooltipTrigger _tooltip;
        [SerializeField] private Image _bg;
        [SerializeField] private GameObject _increaseBtn;
        [SerializeField] private GameObject _decreaseBtn;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _redBG;

        public void OnIncreasePressed()
        {
            
        }

        public void OnDecreasePressed()
        {
            
        }
    }
}
