using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class ClickableObject : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer _sprite;

    private Material _material;
    private int _fadePropertyID;
    private bool _isOutlineLocked;

    protected virtual void Awake()
    {
        _material = _sprite.material;
        _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
    }

    public void TriggerOutline(bool showOuline)
    {
        if (showOuline)
        {
            _material.SetFloat(_fadePropertyID, 1);
        }
        else
        {
            _material.SetFloat(_fadePropertyID, 0);
        }
    }

    public void LockOutline(bool isLocked, bool showOutline)
    {
        _isOutlineLocked = isLocked;
        TriggerOutline(showOutline);
    }

    private void OnMouseEnter()
    {
        if(_isOutlineLocked) return;
        
        TriggerOutline(true);
    }

    private void OnMouseExit()
    {
        if(_isOutlineLocked) return;
        
        TriggerOutline(false);
    }

    private void OnMouseDown()
    {
        OnClicked();
    }

    protected abstract void OnClicked();
}
