using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class UIManager : MonoBehaviour
{
    EventSystem m_EventSystem;
    
    public static UIManager Instance
    {
        get;
        private set;
    }

    UIManager()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        m_EventSystem = GetComponent<EventSystem>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	}

    public bool UIIsMouseover()
    {
        return m_EventSystem.IsPointerOverGameObject();
    }
}
