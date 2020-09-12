using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;
using Pathing;
using Selectable.Units;
using Tools;
using UnityEngine.UI;

namespace Players
{

    [RequireComponent(typeof(Camera))]
    public class UnitSelection : MonoBehaviour
    {
        public int m_PlayerNumber;
        public GameObject m_SelectionCanvasPrefab;

        public LayerMask m_SelectionLayer;
        public GameObject m_SelectionContentObject;

        //public float ClickToHoldTime = 0.25f;
        bool m_IsDragging = false;
        bool m_DenySelect = false;
        Vector3 m_MouseStartPos = Vector3.zero;

        Camera m_Cam;

        public HashSet<Owner> SelectedObjects
        {
            get;
            private set;
        }

        List<Unit> m_AllControlledUnits;// = new List<List<Unit>>(8);

        Owner m_PreviousMouseover;

        public void AddUnit(Unit _u)
        {
            m_AllControlledUnits.Add(_u);
        }

        public void RemoveUnit(Unit _u)
        {
            m_AllControlledUnits.Remove(_u);
        }

        public KeyCode[] m_Keycodes = new KeyCode[0];

        //Vector2 clickLoc = Vector2.zero;
        public Vector2 m_MinDragSize = Vector2.one * 10;

        private void Awake()
        {
            SelectedObjects = new HashSet<Owner>();
            m_AllControlledUnits = new List<Unit>();
        }

        // Use this for initialization
        void Start()
        {
            GameObject g = (GameObject)Instantiate(m_SelectionCanvasPrefab);
            m_SelectionContentObject = g.GetComponentInChildren<GridLayoutGroup>().gameObject;

            m_Cam = gameObject.GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            m_SelectionLayer = m_Cam.cullingMask - 1;

            HandleMouseInput();

            HandleKeyboardInput();
        }

        /// <summary>
        /// Handles mouse input for the purpose of Unit Selection
        /// </summary>
        void HandleMouseInput()
        {
            if (Input.GetButtonDown("SelectDeselect"))
            {
                if (UIManager.Instance.UIIsMouseover())
                    m_DenySelect = true;
                m_MouseStartPos = Input.mousePosition;
            }
            else if (Input.GetButton("SelectDeselect") == true)
            {
                if (m_IsDragging == false && !m_DenySelect && (Input.mousePosition - m_MouseStartPos).sqrMagnitude > m_MinDragSize.sqrMagnitude)
                {
                    m_IsDragging = true;
                    StartCoroutine(DragHighlight());
                }
            }
            else if (Input.GetButtonUp("SelectDeselect"))
            {
                if (!m_DenySelect)
                {
                    if (m_IsDragging == false)
                    {
                        ClickSelect();
                    }
                    else
                    {
                        m_IsDragging = false;
                    }
                }
                m_DenySelect = false;
            }

            if (Input.GetButtonDown("GiveCommand"))
            {
                if (!UIManager.Instance.UIIsMouseover())
                    IssueCommand();
            }

            if (Input.GetButton("SelectDeselect") == false && Input.GetButton("GiveCommand") == false)
            {
                if (!UIManager.Instance.UIIsMouseover())
                    MouseOver();
            }
        }

        void HandleKeyboardInput()
        {
            foreach (KeyCode k in m_Keycodes)
            {
                if (Input.GetKeyDown(k))
                {
                    foreach (Owner o in SelectedObjects)
                    {
                        o.OnKeyDown(k);
                    }
                }
                else if (Input.GetKeyUp(k))
                {
                    foreach (Owner o in SelectedObjects)
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
            Ray ray = m_Cam.ScreenPointToRay(m_MouseStartPos);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1024f, m_SelectionLayer);

            Owner closest = null;
            float dist = float.MaxValue;

            foreach (RaycastHit h in hits)
            {
                Owner o = h.transform.GetComponent<Owner>();
                if (o == null)
                    continue;

                float newDist = Vector3.SqrMagnitude(Input.mousePosition - m_Cam.WorldToViewportPoint(gameObject.transform.position));
                if (newDist < dist)
                {
                    closest = h.transform.GetComponent<Owner>();
                    dist = newDist;
                }
            }

            if (closest == null)
            {
                DeselectOld();
                return;
            }

            if (Input.GetButton("AddSelection") == false)
            {
                //Deselect all other units
                DeselectOld();
            }
            HashSet<Owner>.Enumerator e = SelectedObjects.GetEnumerator();
            e.MoveNext();
            if (SelectedObjects.Count > 0 && closest.PlayerNumber != e.Current.PlayerNumber)
            {
                DeselectOld();
            }

            //Deselect the single unit if already selected
            if (SelectedObjects.Contains(closest))
            {
                DeselectSingle(closest);
                SelectedObjects.Remove(closest);
            }
            //Otherwise, select the single unit
            else
            {
                SelectNew(closest);
            }
        }

        void IssueCommand()
        {
            //Do collision detection to see if the mouse is over a target or not.

            //For now, all commands will assume a simple move command

            List<Owner>[] selectedDims = new List<Owner>[3] { new List<Owner>(), new List<Owner>(), new List<Owner>() };

            foreach (Owner o in SelectedObjects)
            {
                selectedDims[o.gameObject.layer - 8].Add(o);
            }

            for (int i = 0; i < 3; ++i)
            {
                if (selectedDims[i].Count == 0)
                    continue;

                RaycastHit hit;
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1024f, TerrainData.Layers.DimAndDefault_s(i + 1)))
                {
                    Goal newGoal;
                    Owner ownerComp = hit.transform.GetComponent<Selectable.Owner>();
                    if (ownerComp != null)
                        newGoal = new Goal(m_PlayerNumber, (char)i, hit.transform);
                    else
                        newGoal = new Goal(m_PlayerNumber, (char)i, hit.point);

                    foreach (Owner o in selectedDims[i])
                    {
                        o.SetTargetGoal(newGoal);

                        o.OnRightMouse();
                    }
                }
            }
        }

        /// <summary>
        /// Handles logic for when mousing over any object when no button is down
        /// </summary>
        void MouseOver()
        {
            Ray ray = m_Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1024f, m_SelectionLayer);

            Owner closest = null;
            float dist = float.MaxValue;

            foreach (RaycastHit h in hits)
            {
                Owner o = h.transform.GetComponent<Owner>();
                if (o == null)
                    continue;

                float newDist = Vector3.SqrMagnitude(Input.mousePosition - m_Cam.WorldToViewportPoint(gameObject.transform.position));
                if (newDist < dist)
                {
                    closest = h.transform.GetComponent<Owner>();
                    dist = newDist;
                }
            }

            if (m_PreviousMouseover != null)
                m_PreviousMouseover.SetHighlighted(false);

            if (closest == null)
            {
                return;
            }

            m_PreviousMouseover = closest;

            closest.SetHighlighted(true);
        }

        /// <summary>
        /// Coroutine for handling while the mouse is held down
        /// </summary>
        /// <returns>Standard coroutine return. Loops (returns null) while mouse is held down</returns>
        IEnumerator DragHighlight()
        {
            HashSet<Unit> highlightedUnits = new HashSet<Unit>();

            //While holding left mouse button
            while (Input.GetButton("SelectDeselect"))
            {
                //Get every Unit type object
                foreach (Unit u in m_AllControlledUnits)
                {
                    int SelectLayer = Utils.LayerMaskToInt_s(m_SelectionLayer);
                    int UnitLayer = Utils.ObjectLayerToInt_s(u.gameObject.layer);

                    //If it's owned by the player and is within the selection box in the current viewed dimensions, add it to be highlighted, otherwise unhighlight it
                    if ((SelectLayer & UnitLayer) != 0 && IsWithinSelectionBounds(u.gameObject))
                    {
                        highlightedUnits.Add(u);
                        //Don't highlight if already selected
                        //if(selectedObjects.Contains(u.GetComponent<Owner>()) == false)
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
            if (Input.GetButton("AddSelection") == false)
            {
                //Debug.Log("Shift not pressed");
                //Deselect all other units
                DeselectOld();
            }
            HashSet<Owner>.Enumerator e = SelectedObjects.GetEnumerator();
            e.MoveNext();
            if (SelectedObjects.Count > 0 && e.Current.PlayerNumber != m_PlayerNumber)
            {
                DeselectOld();
            }

            //Select all units not already selected
            foreach (Unit u in highlightedUnits)
            {
                Owner o = u.GetComponent<Owner>();
                o.SetHighlighted(false);
                if (SelectedObjects.Contains(o) == false)
                    SelectNew(o);
            }
        }

        /// <summary>
        /// Deselects all objects already selected
        /// </summary>
        internal void DeselectOld()
        {
            foreach (Owner o in SelectedObjects)
            {
                o.RemoveOnDeathCall(DeselectSingle);
                o.Deselect();
                o.RemoveSelectionIcon(m_PlayerNumber);
            }
            SelectedObjects.Clear();
        }

        /// <summary>
        /// Deselects this given object
        /// </summary>
        /// <param name="_target">The object to remove from selection</param>
        public void DeselectSingle(Owner _target)
        {
            _target.RemoveOnDeathCall(DeselectSingle);
            _target.Deselect();
            _target.RemoveSelectionIcon(m_PlayerNumber);
            SelectedObjects.Remove(_target);
        }

        /// <summary>
        /// Adds a new object to the list of selected objects
        /// </summary>
        /// <param name="_newlySelected">The new object that was selected</param>
        void SelectNew(Owner _newlySelected)
        {
            //Debug.Log("Selected: " + newlySelected.name);
            _newlySelected.AddOnDeathCall(DeselectSingle);
            _newlySelected.Select();
            SelectedObjects.Add(_newlySelected);
            _newlySelected.SetHighlighted(false);
            
            GameObject icon = Instantiate(_newlySelected.m_SelectionIconPrefab);
            icon.GetComponent<Image>().sprite = _newlySelected.GetImage;
            _newlySelected.AddSelectionIcon(m_PlayerNumber, icon);
            icon.transform.SetParent(m_SelectionContentObject.transform);
            icon.transform.localScale = Vector3.one;
        }

        public void DeselectAllOthers(Owner _onlySelect)
        {
            DeselectOld();
            SelectNew(_onlySelect);
        }

        /// <summary>
        /// Checks if a given game object's position is within the selection bounds
        /// </summary>
        /// <param name="_gameObject">The game object to check the position of</param>
        /// <returns>Returns True if the position is within the selection bounds</returns>
        public bool IsWithinSelectionBounds(GameObject _gameObject)
        {
            var viewportBounds = Utils.GetViewportBounds_s(m_Cam, m_MouseStartPos, Input.mousePosition);

            return viewportBounds.Contains(m_Cam.WorldToViewportPoint(_gameObject.transform.position));
        }

        void OnGUI()
        {
            if (m_IsDragging)
            {
                // Create a rect from both mouse positions
                var rect = Utils.GetScreenRect_s(m_MouseStartPos, Input.mousePosition);
                //Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder_s(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }
    }

}
