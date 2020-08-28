using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

delegate bool MyFunc<T1, T2>(T1 a, out T2 b);

public class SelectionClickHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnSingleClickDelegate(GameObject obj);
    public event OnSingleClickDelegate singleClickDelegate = delegate { };

    public delegate void OnDoubleClickDelegate(GameObject obj);
    public event OnDoubleClickDelegate doubleClickDelegate = delegate { };

    public delegate void OnRightClickDelegate(GameObject obj);
    public event OnRightClickDelegate rightClickDelegate = delegate { };

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                singleClickDelegate(gameObject);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                rightClickDelegate(gameObject);
            }
        }
        if (eventData.clickCount == 2)
        {
            doubleClickDelegate(gameObject);
        }

    }

    public void AddSingleClick(OnSingleClickDelegate method)
    {
        singleClickDelegate += method;
    }
    public void RemoveSingleClick(OnSingleClickDelegate method)
    {
        singleClickDelegate -= method;
    }

    public void AddDoubleClick(OnDoubleClickDelegate method)
    {
        doubleClickDelegate += method;
    }
    public void RemoveDoubleClick(OnDoubleClickDelegate method)
    {
        doubleClickDelegate -= method;
    }

    public void AddRightClick(OnRightClickDelegate method)
    {
        rightClickDelegate += method;
    }
    public void RemoveRightClick(OnRightClickDelegate method)
    {
        rightClickDelegate -= method;
    }
}
