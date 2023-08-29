using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent OnCursorEnter;
    public UnityEvent OnCursorExit;
    
    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        OnCursorEnter.Invoke();
    }

    private void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        OnCursorExit.Invoke();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        OnClick.Invoke();
    }
}
