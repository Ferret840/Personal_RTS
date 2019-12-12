using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    public LayerMask SelectionLayer;
    int PlayerNumber = 0;

    //public float ClickToHoldTime = 0.25f;
    bool IsDragging = false;
    Vector3 MouseStartPos = Vector3.zero;

    Camera cam;

    HashSet<Owner> selectedObjects = new HashSet<Owner>();

    static List<List<Unit>> AllUnits;// = new List<List<Unit>>(8);

    static public void AddUnit(Unit u)
    {
        AllUnits[u.ControlledByPlayerNum].Add(u);
    }

    static public void RemoveUnit(Unit u)
    {
        AllUnits[u.ControlledByPlayerNum].Remove(u);
    }

    public KeyCode[] keycodes = new KeyCode[0];

    //Vector2 clickLoc = Vector2.zero;
    public Vector2 minDragSize = Vector2.one * 10;

    private void Awake()
    {
        if (AllUnits == null)
        {
            AllUnits = new List<List<Unit>>(8);

            for (int i = 0; i < AllUnits.Capacity; ++i)
            {
                AllUnits.Add(new List<Unit>(8));
            }
        }
    }

    // Use this for initialization
    void Start ()
    {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
    {
        SelectionLayer = cam.cullingMask - 1;

        HandleMouseInput();

        HandleKeyboardInput();
    }

    /// <summary>
    /// Handles mouse input for the purpose of Unit Selection
    /// </summary>
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) == true)
        {
            if (IsDragging == false && (Input.mousePosition - MouseStartPos).sqrMagnitude > minDragSize.sqrMagnitude)
            {
                IsDragging = true;
                StartCoroutine(DragHighlight());
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (IsDragging == false)
            {
                ClickSelect();
            }
            else
            {
                IsDragging = false;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Owner o in selectedObjects)
            {
                o.OnRightMouse();
            }
        }
    }

    void HandleKeyboardInput()
    {
        foreach (KeyCode k in keycodes)
        {
            if (Input.GetKeyDown(k))
            {
                foreach (Owner o in selectedObjects)
                {
                    o.OnKeyDown(k);
                }
            }
            else if (Input.GetKeyUp(k))
            {
                foreach (Owner o in selectedObjects)
                {
                    o.OnKeyUp(k);
                }
            }
        }
    }

    /// <summary>
    /// Handles logic for when selecting a singular object without dragging
    /// </summary>
    void ClickSelect()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(MouseStartPos);
        bool ifCastHit = Physics.Raycast(ray, out hit, 1024f, SelectionLayer);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1024f, SelectionLayer);

        Owner closest = null;
        float dist = float.MaxValue;

        foreach (RaycastHit h in hits)
        {
            Owner o = h.transform.GetComponent<Owner>();
            if (o == null)
                continue;

            float newDist = Vector3.SqrMagnitude(Input.mousePosition - cam.WorldToViewportPoint(gameObject.transform.position));
            if (newDist < dist)
            {
                closest = h.transform.GetComponent<Owner>();
                dist = newDist;
            }
        }

        if (closest == null)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift) == false && Input.GetKey(KeyCode.RightShift) == false)
        {
            //Deselect all other units
            DeselectOld();
        }

        //Deselect the single unit if already selected
        if (selectedObjects.Contains(closest))
        {
            closest.Deselect();
            selectedObjects.Remove(closest);
        }
        //Otherwise, select the single unit
        else
        {
            SelectNew(closest);
        }
    }

    /// <summary>
    /// Coroutine for handling while the mouse is held down
    /// </summary>
    /// <returns>Standard coroutine return. Loops (returns null) while mouse is held down</returns>
    IEnumerator DragHighlight()
    {
        HashSet<Unit> highlightedUnits = new HashSet<Unit>();

        //While holding left mouse button
        while (Input.GetMouseButton(0))
        {
            //Get every Unit type object
            foreach (Unit u in AllUnits[PlayerNumber])
            {
                int SelectLayer = Utils.LayerMaskToInt(SelectionLayer);
                int UnitLayer = Utils.ObjectLayerToInt(u.gameObject.layer);

                //If it's owned by the player and is within the selection box in the current viewed dimensions, add it to be highlighted, otherwise unhighlight it
                if (u.ControlledByPlayerNum == PlayerNumber && (SelectLayer & UnitLayer) != 0 && IsWithinSelectionBounds(u.gameObject))
                {
                    highlightedUnits.Add(u);
                    //Don't highlight if already selected
                    if(selectedObjects.Contains(u.GetComponent<Owner>()) == false)
                        u.SetHighlighted(true);
                }
                else
                {
                    highlightedUnits.Remove(u);
                    u.SetHighlighted(false);
                }
            }
            yield return null;
        }

        //If no shift key is down
        if (Input.GetKey(KeyCode.LeftShift) == false && Input.GetKey(KeyCode.RightShift) == false)
        {
            //Debug.Log("Shift not pressed");
            //Deselect all other units
            DeselectOld();
        }

        //Select all units not already selected
        foreach (Unit u in highlightedUnits)
        {
            Owner o = u.GetComponent<Owner>();
            if (selectedObjects.Contains(o) == false)
                SelectNew(o);
        }
    }

    /// <summary>
    /// Deselects all objects already selected
    /// </summary>
    void DeselectOld()
    {
        foreach (Owner o in selectedObjects)
        {
            o.onDied -= DeselectSingle;
            o.Deselect();
        }
        selectedObjects.Clear();
    }

    /// <summary>
    /// Deselects this given object
    /// </summary>
    /// <param name="target">The object to remove from selection</param>
    void DeselectSingle(Owner target)
    {
        target.onDied -= DeselectSingle;
        target.Deselect();
        selectedObjects.Remove(target);
    }

    /// <summary>
    /// Adds a new object to the list of selected objects
    /// </summary>
    /// <param name="newlySelected">The new object that was selected</param>
    void SelectNew(Owner newlySelected)
    {
        //Debug.Log("Selected: " + newlySelected.name);
        newlySelected.onDied += DeselectSingle;
        newlySelected.Select();
        selectedObjects.Add(newlySelected);
        newlySelected.SetHighlighted(false);
    }

    /// <summary>
    /// Checks if a given game object's position is within the selection bounds
    /// </summary>
    /// <param name="gameObject">The game object to check the position of</param>
    /// <returns>Returns True if the position is within the selection bounds</returns>
    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        var viewportBounds = Utils.GetViewportBounds(cam, MouseStartPos, Input.mousePosition);

        return viewportBounds.Contains(cam.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (IsDragging)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(MouseStartPos, Input.mousePosition);
            //Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}

public static class Utils
{
    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    /// <summary>
    /// Takes a LayerMask and converts it to an int containing which dimensions are exposed to that LayerMask.
    /// (i.e. It can see Dimension 1 and/or Dimension 2)
    /// </summary>
    /// <param name="original">The original LayerMask value</param>
    /// <returns>Returns an int containing the exposed dimensions</returns>
    public static int LayerMaskToInt(LayerMask original)
    {
        int dim = 0;

        if ((original & (1 << 8)) != 0)
            dim |= 1;
        if ((original & (1 << 9)) != 0)
            dim |= 2;

        return dim;
    }

    /// <summary>
    /// Converts the given Layer of an object to which dimension the object exists in.
    /// (i.e. It's in Dimension 1, 2, or Both (3))
    /// </summary>
    /// <param name="original">The original Layer value of the object</param>
    /// <returns>Returns the game logic value for dimensions</returns>
    public static int ObjectLayerToInt(int original)
    {
        int dim = 0;

        if (original == 8)
            dim |= 1;
        if (original == 9)
            dim |= 2;
        if (original == 10)
            dim |= 3;

        return dim;
    }

    public static int IntToLayer(int original)
    {
        return original + 7;
    }
}
