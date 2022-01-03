using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private GameObject _barFrame;
    [SerializeField] private GameObject _bar;

    private float _fullWidth;

    public void ShowBar(bool showBar)
    {
        _barFrame.SetActive(showBar);
        SetProgress(0f);
    }
    
    public void SetProgress(float percent)
    {
        var xScale = _fullWidth * percent;
        xScale = Mathf.Clamp( xScale, 0f, _fullWidth);

        _bar.transform.localScale = new Vector3(xScale, _bar.transform.localScale.y, _bar.transform.localScale.z);
    }

    private void Awake()
    {
        _fullWidth = _bar.transform.localScale.x;
    }
}
