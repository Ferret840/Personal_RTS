﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;
using Pathing;
using Selectable.Units;
using Tools;

namespace Players
{

    [RequireComponent(typeof(Camera))]
    public class UnitSelection : MonoBehaviour
    {
        public int PlayerNumber;

        public LayerMask SelectionLayer;

        //public float ClickToHoldTime = 0.25f;
        bool isDragging = false;
        Vector3 mouseStartPos = Vector3.zero;

        Camera cam;

        public HashSet<Owner> selectedObjects
        {
            get;
            private set;
        }

        List<Unit> allControlledUnits;// = new List<List<Unit>>(8);

        Owner previousMouseover;

        public void AddUnit(Unit u)
        {
            allControlledUnits.Add(u);
        }

        public void RemoveUnit(Unit u)
        {
            allControlledUnits.Remove(u);
        }

        public KeyCode[] Keycodes = new KeyCode[0];

        //Vector2 clickLoc = Vector2.zero;
        public Vector2 MinDragSize = Vector2.one * 10;

        private void Awake()
        {
            selectedObjects = new HashSet<Owner>();
            allControlledUnits = new List<Unit>();
        }

        // Use this for initialization
        void Start()
        {
            cam = gameObject.GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
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
            if (Input.GetButtonDown("SelectDeselect"))
            {
                mouseStartPos = Input.mousePosition;
            }
            else if (Input.GetButton("SelectDeselect") == true)
            {
                if (isDragging == false && (Input.mousePosition - mouseStartPos).sqrMagnitude > MinDragSize.sqrMagnitude)
                {
                    isDragging = true;
                    StartCoroutine(DragHighlight());
                }
            }
            else if (Input.GetButtonUp("SelectDeselect"))
            {
                if (isDragging == false)
                {
                    ClickSelect();
                }
                else
                {
                    isDragging = false;
                }
            }

            if (Input.GetButtonDown("GiveCommand"))
            {
                //Do collision detection to see if the mouse is over a target or not.

                //For now, all commands will assume a simple move command

                List<Owner>[] selectedDims = new List<Owner>[3] { new List<Owner>(), new List<Owner>(), new List<Owner>() };

                foreach (Owner o in selectedObjects)
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
                    if (Physics.Raycast(ray, out hit, 1024f))
                    {
                        Goal newGoal = new Goal(PlayerNumber, (char)i, hit.point);

                        foreach (Owner o in selectedDims[i])
                        {
                            o.SetTargetGoal(newGoal);

                            o.OnRightMouse();
                        }
                    }
                }
            }

            if (Input.GetButton("SelectDeselect") == false && Input.GetButton("GiveCommand") == false)
            {
                MouseOver();
            }
        }

        void HandleKeyboardInput()
        {
            foreach (KeyCode k in Keycodes)
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
            Ray ray = cam.ScreenPointToRay(mouseStartPos);
            Physics.Raycast(ray, out hit, 1024f, SelectionLayer);
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
                DeselectOld();
                return;
            }

            if (Input.GetButton("AddSelection") == false)
            {
                //Deselect all other units
                DeselectOld();
            }
            HashSet<Owner>.Enumerator e = selectedObjects.GetEnumerator();
            e.MoveNext();
            if (selectedObjects.Count > 0 && closest.PlayerNumber != e.Current.PlayerNumber)
            {
                DeselectOld();
            }

            //Deselect the single unit if already selected
            if (selectedObjects.Contains(closest))
            {
                DeselectSingle(closest);
                selectedObjects.Remove(closest);
            }
            //Otherwise, select the single unit
            else
            {
                SelectNew(closest);
            }
        }

        /// <summary>
        /// Handles logic for when mousing over any object when no button is down
        /// </summary>
        void MouseOver()
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, 1024f, SelectionLayer);
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

            if (previousMouseover != null)
                previousMouseover.SetHighlighted(false);

            if (closest == null)
            {
                return;
            }

            previousMouseover = closest;

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
                foreach (Unit u in allControlledUnits)
                {
                    int SelectLayer = Utils.LayerMaskToInt(SelectionLayer);
                    int UnitLayer = Utils.ObjectLayerToInt(u.gameObject.layer);

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
            HashSet<Owner>.Enumerator e = selectedObjects.GetEnumerator();
            e.MoveNext();
            if (selectedObjects.Count > 0 && e.Current.PlayerNumber != PlayerNumber)
            {
                DeselectOld();
            }

            //Select all units not already selected
            foreach (Unit u in highlightedUnits)
            {
                Owner o = u.GetComponent<Owner>();
                o.SetHighlighted(false);
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
            var viewportBounds = Utils.GetViewportBounds(cam, mouseStartPos, Input.mousePosition);

            return viewportBounds.Contains(cam.WorldToViewportPoint(gameObject.transform.position));
        }

        void OnGUI()
        {
            if (isDragging)
            {
                // Create a rect from both mouse positions
                var rect = Utils.GetScreenRect(mouseStartPos, Input.mousePosition);
                //Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }
    }

}