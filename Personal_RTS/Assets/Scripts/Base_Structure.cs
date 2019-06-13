using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_Structure : NonTerrainObstacle
{
    // Use this for initialization
    void Start()
    {
        Init();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (IsSelected && Input.GetKeyDown(KeyCode.Delete))
        {
            TakeDamage(1);
        }
    }

    override public void Deselect()
    {
        base.Deselect();
        //owner.SelectedEffect.SetActive(false);
        //owner.IsSelected = false;
    }

    override public void Select()
    {
        base.Select();
        //owner.SelectedEffect.SetActive(true);
        //owner.IsSelected = true;
    }

    override public void SetHighlighted(bool IsHighlighted)
    {
        base.SetHighlighted(IsHighlighted);
        //owner.HighlightedEffect.SetActive(IsHighlighted);
    }
}
