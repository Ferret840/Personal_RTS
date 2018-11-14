using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owner : MonoBehaviour
{
    public int OwnByPlayerNum;
    [System.NonSerialized]
    public bool IsSelected = false;

    public GameObject SelectedEffect;
    public GameObject HighlightedEffect;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    virtual public void Deselect()
    {
        print("O De");
    }

    virtual public void Select()
    {
        print("O Se");
    }

    virtual public void SetHighlighted(bool IsHighlighted)
    {

    }
}
