using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HUD.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Header;
        
        [Multiline()]
        public string Content;

        private static LTDescr delay;
        private bool _isShowingTooltip;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(Content, Header);
                _isShowingTooltip = true;
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LeanTween.cancel(delay.uniqueId);
            TooltipSystem.Hide();
            _isShowingTooltip = false;
        }

        public void OnMouseEnter()
        {
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(Content, Header);
                _isShowingTooltip = true;
            });
        }

        public void OnMouseExit()
        {
            LeanTween.cancel(delay.uniqueId);
            TooltipSystem.Hide();
            _isShowingTooltip = false;
        }

        private void OnDestroy()
        {
            if (_isShowingTooltip)
            {
                LeanTween.cancel(delay.uniqueId);
                TooltipSystem.Hide();
                _isShowingTooltip = false;
            }
        }
    }
}
