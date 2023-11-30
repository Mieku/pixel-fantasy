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
            _isShowingTooltip = true;
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(Content, Header);
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
            _isShowingTooltip = true;
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(Content, Header);
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

        private void OnDisable()
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
