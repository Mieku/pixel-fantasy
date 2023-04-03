using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class ClickableObject : MonoBehaviour
{
    protected SpriteRenderer _spriteToOutline;

    private Material _material;
    private int _fadePropertyID;

    protected virtual void Awake()
    {
        _material = _spriteToOutline.material;
        _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
    }

    private void TriggerOutline(bool showOuline)
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

    private void OnMouseEnter()
    {
        TriggerOutline(true);
    }

    private void OnMouseExit()
    {
        TriggerOutline(false);
    }

    private void OnMouseDown()
    {
        OnClicked();
    }

    protected abstract void OnClicked();
}
