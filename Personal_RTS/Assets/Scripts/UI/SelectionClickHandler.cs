using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionClickHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void SingleClickDelegate(GameObject _obj);
    event SingleClickDelegate OnSingleClickEvent = delegate { };

    public delegate void DoubleClickDelegate(GameObject _obj);
    event DoubleClickDelegate OnDoubleClickEvent = delegate { };

    public delegate void RightClickDelegate(GameObject _obj);
    event RightClickDelegate OnRightClickEvent = delegate { };

    public void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData.clickCount == 1)
        {
            if (_eventData.button == PointerEventData.InputButton.Left)
            {
                OnSingleClickEvent(gameObject);
            }
            else if (_eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClickEvent(gameObject);
            }
        }
        if (_eventData.clickCount == 2)
        {
            OnDoubleClickEvent(gameObject);
        }

    }

    public void AddSingleClick(SingleClickDelegate _method)
    {
        OnSingleClickEvent += _method;
    }
    public void RemoveSingleClick(SingleClickDelegate _method)
    {
        OnSingleClickEvent -= _method;
    }

    public void AddDoubleClick(DoubleClickDelegate _method)
    {
        OnDoubleClickEvent += _method;
    }
    public void RemoveDoubleClick(DoubleClickDelegate _method)
    {
        OnDoubleClickEvent -= _method;
    }

    public void AddRightClick(RightClickDelegate _method)
    {
        OnRightClickEvent += _method;
    }
    public void RemoveRightClick(RightClickDelegate _method)
    {
        OnRightClickEvent -= _method;
    }
}
