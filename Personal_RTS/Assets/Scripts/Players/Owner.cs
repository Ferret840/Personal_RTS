using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owner : MonoBehaviour
{
    [SerializeField]
    public int PlayerNumber
    {
        get;
        protected set;
    }

    public int EditorSetPlayerNum;

    [SerializeField]
    protected UniversalStats univStats;

    [System.NonSerialized]
    public bool IsSelected = false;

    public GameObject SelectedEffect;
    public GameObject HighlightedEffect;


    public delegate void OnDied(Owner owner);
    public event OnDied onDied;

    public LayerMask ValidMovementLayers;

    // Use this for initialization
    void Awake()
    {
        PlayerNumber = EditorSetPlayerNum;
    }

    // Use this for initialization
    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    virtual public void OnRightMouse()
    {

    }

    /// <summary>
    /// Overrideable function for handling when a key is pressed
    /// </summary>
    /// <param name="k">An int within Keycode corresponding to the key that was pressed</param>
    virtual public void OnKeyDown(KeyCode k)
    {
        
    }

    /// <summary>
    /// Overrideable function for handling when a key is released
    /// </summary>
    /// <param name="k">An int within Keycode corresponding to the key that was released</param>
    virtual public void OnKeyUp(KeyCode k)
    {

    }

    virtual public void Deselect()
    {
        SelectedEffect.SetActive(false);
        IsSelected = false;
        //print("O De");
    }

    virtual public void Select()
    {
        SelectedEffect.SetActive(true);
        IsSelected = true;
        //print("O Se");
    }

    virtual public void SetHighlighted(bool IsHighlighted)
    {
        HighlightedEffect.SetActive(IsHighlighted);
    }

    virtual public void TakeDamage(int incomingDamage)
    {
        if (incomingDamage >= univStats.Health)
        {
            HandleDeath();
        }
        univStats.Health -= incomingDamage;
    }

    virtual protected void OnDestroy()
    {
    }

    virtual protected void HandleDeath()
    {
        onDied(this);
        Destroy(gameObject);
    }
}

[System.Serializable]
public class UniversalStats
{
    public int Cost = 100;
    public float BuildTime = 1.0f;

    public int Health = 1;
}
