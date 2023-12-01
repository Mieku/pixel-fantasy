using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent OnLeftClick;
    public UnityEvent OnRightClick;
    public UnityEvent OnMiddleClick;
    public UnityEvent OnHoverStart;
    public UnityEvent OnHoverEnd;

    private bool _mouseOver;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            OnMiddleClick.Invoke();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseOver = true;
        OnHoverStart.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_mouseOver)
        {
            _mouseOver = false;
            OnHoverEnd.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (_mouseOver)
        {
            _mouseOver = false;
            OnHoverEnd.Invoke();
        }
    }

    private void OnDisable()
    {
        if (_mouseOver)
        {
            _mouseOver = false;
            OnHoverEnd.Invoke();
        }
    }
}
