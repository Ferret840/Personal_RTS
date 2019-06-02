using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    public LayerMask SelectionLayer;
    int PlayerNumber = 0;

    public float ClickToHoldTime = 0.25f;
    bool IsSelecting = false;
    Vector3 MouseStartPos = Vector3.zero;

    Camera cam;

    HashSet<Owner> selectedUnits = new HashSet<Owner>();

	// Use this for initialization
	void Start ()
    {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseStartPos = Input.mousePosition;
            StartCoroutine(CheckForHold());
        }
        SelectionLayer = cam.cullingMask - 1;

        //string AllNames = "";
        //foreach (Owner o in selectedUnits)
        //{
        //    AllNames = AllNames + o.name + ", ";
        //}
        //print(AllNames);
    }

    IEnumerator CheckForHold()
    {
        float t = 0f;

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(MouseStartPos);
        bool ifCastHit = Physics.Raycast(ray, out hit, 512f, SelectionLayer);

        HashSet<Owner> highlightedUnits = new HashSet<Owner>();

        //While holding left mouse button
        while (Input.GetMouseButton(0))
        {
            //Increment time to see if we should draw a box selector or not
            t += Time.deltaTime;
            if (t > ClickToHoldTime)
            {
                IsSelecting = true;

                //Get every Owner type object
                foreach (Unit u in FindObjectsOfType<Owner>())
                {
                    int SelectLayer = Utils.LayerMaskToInt(SelectionLayer);
                    int UnitLayer = Utils.ObjectLayerToInt(u.gameObject.layer);
                    //If it's owned by the player and is within the selection box in the current viewed dimensions, add it to be highlighted, otherwise unhighlight it
                    if (u.OwnByPlayerNum == PlayerNumber && IsWithinSelectionBounds(u.gameObject) && (SelectLayer & UnitLayer) != 0)
                    {
                        highlightedUnits.Add(u);
                        //Don't highlight if already selected
                        if(selectedUnits.Contains(u) == false)
                            u.SetHighlighted(true);
                    }
                    else
                    {
                        highlightedUnits.Remove(u);
                        u.SetHighlighted(false);
                    }
                }
            }
            yield return null;
        }

        //If no shift key is down
        if (Input.GetKey(KeyCode.LeftShift) == false && Input.GetKey(KeyCode.RightShift) == false)
        {
            //Deselect all other units
            DeselectOld();
        }

        //When holding shfit and targeting a single unit
        if (t < ClickToHoldTime)
        {
            if (ifCastHit)
            {
                Owner newlySelected = hit.transform.GetComponent<Owner>();

                //Deselect the single unit
                if (selectedUnits.Contains(newlySelected))
                {
                    newlySelected.Deselect();
                    selectedUnits.Remove(newlySelected);
                }
                //Select the single unit
                else
                {
                    SelectNew(newlySelected);
                }
            }
        }
        //When holding shfit and doing a box selection
        else
        {
            //Select all units not already selected
            foreach (Owner o in highlightedUnits)
            {
                if(selectedUnits.Contains(o) == false)
                    SelectNew(o);
            }
        }

        IsSelecting = false;
    }

    void DeselectOld()
    {
        foreach (Owner o in selectedUnits)
        {
            o.Deselect();
        }
        selectedUnits.Clear();
    }

    void SelectNew(Owner newlySelected)
    {
        newlySelected.Select();
        selectedUnits.Add(newlySelected);
        newlySelected.SetHighlighted(false);
    }

    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        var viewportBounds = Utils.GetViewportBounds(cam, MouseStartPos, Input.mousePosition);

        return viewportBounds.Contains(cam.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (IsSelecting)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(MouseStartPos, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
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
}
