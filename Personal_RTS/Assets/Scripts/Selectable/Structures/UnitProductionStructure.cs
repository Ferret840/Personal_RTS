using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProductionStructure : Base_Structure
{
    [SerializeField]
    GameObject objectBase;

    //public UnitVariables[] SpawnObject = new UnitVariables[0];
    public ProductionInfo[] SpawnObjects = new ProductionInfo[0];

    Vector3 RallyPointLocation = Vector3.one;

    //Queue<UnitVariables> queuedUnits = new Queue<UnitVariables>();
    Queue<ProductionInfo> unitQueue = new Queue<ProductionInfo>();

    float BuildPercent = 1;

	// Use this for initialization
	void Start ()
    {
        Init();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    override public void OnRightMouse()
    {
        base.OnRightMouse();

        RaycastHit hit;
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 512f, ValidMovementLayers))
        {
            RallyPointLocation = hit.point;
        }
    }

    /// <summary>
    /// Overrideable function for handling when a key is pressed
    /// </summary>
    /// <param name="k">An int within Keycode corresponding to the key that was pressed</param>
    override public void OnKeyDown(KeyCode k)
    {
        if (k >= KeyCode.Keypad0 && k <= KeyCode.Keypad9)
        {
            //queuedUnits.Enqueue(SpawnObject[k - KeyCode.Keypad0]);
            //if (queuedUnits.Count > 1)
            unitQueue.Enqueue(SpawnObjects[k - KeyCode.Keypad0]);
            if(unitQueue.Count == 1)
            {
                StartCoroutine(BuildUnit());
            }
        }
    }

    /// <summary>
    /// Overrideable function for handling when a key is released
    /// </summary>
    /// <param name="k">An int within Keycode corresponding to the key that was released</param>
    override public void OnKeyUp(KeyCode k)
    {

    }

    IEnumerator BuildUnit()
    {
        //while (queuedUnits.Count > 0)
        //{
        //    UnitVariables o = queuedUnits.Peek();
        //    for (float i = 0; i < o.BuildTime; i += Time.deltaTime)
        //    {
        //        BuildPercent = i / o.BuildTime;
        //
        //        yield return null;
        //    }
        //
        //    GameObject newUnit = Instantiate(objectBase);
        //}
        while (unitQueue.Count > 0)
        {
            ProductionInfo o = unitQueue.Peek();
            for (float i = 0; i < o.UnivStats.BuildTime; i += Time.deltaTime)
            {
                BuildPercent = i / o.UnivStats.BuildTime;
                Debug.Log("Training: " + (BuildPercent * 100).ToString("F2") + "%");

                yield return null;
            }

            GameObject newUnit = Instantiate(objectBase);
            newUnit.layer = Utils.IntToLayer(o.UnitVar.dimension);
            //Unit unitComp = newUnit.AddComponent<Unit>();
            Unit unitComp = newUnit.GetComponent<Unit>();
            unitComp.SetUnitStats = o.UnitVar;

            Debug.Log("Unit created");
            yield return new WaitForSeconds(2);
            unitComp.SetMoveLocation(RallyPointLocation);

            unitQueue.Dequeue();
        }
    }
}

[System.Serializable]
public class ProductionInfo
{
    public UniversalStats UnivStats;
    public UnitVariables UnitVar;
}
