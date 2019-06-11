using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Owner))]
public class Base_Structure : NonTerrainObstacle
{
    Owner owner;

    public int OwnByPlayerNum
    {
        get { return owner.OwnByPlayerNum; }
    }

    // Use this for initialization
    void Start()
    {
        Init();
        owner = GetComponent<Owner>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    virtual public void Deselect()
    {
        owner.SelectedEffect.SetActive(false);
        owner.IsSelected = false;
    }

    virtual public void Select()
    {
        owner.SelectedEffect.SetActive(true);
        owner.IsSelected = true;
    }

    virtual public void SetHighlighted(bool IsHighlighted)
    {
        owner.HighlightedEffect.SetActive(IsHighlighted);
    }
}
