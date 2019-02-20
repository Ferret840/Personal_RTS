using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Owner
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public short dimension = 0;

    public Transform target;
    public float speed = 5;
    public float turnDist = 5;
    public float turnSpeed = 3;
    public float stoppingDist = 10;

    public bool drawPath = false;

    public LayerMask ValidMovementLayers;

    Path path;
    //int targetIndex;

    PathRequest pathingRequest;

    private void Start()
    {
        //StartCoroutine(UpdatePath());
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] waypoints, bool PathSuccessful)
    {
        if(PathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDist, stoppingDist);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    /* Needs fixing to have them stop moving after moving to the next grid location, or add an extra point in front to the new path*/
    //IEnumerator UpdatePath()
    //{
    //    if (Time.timeSinceLevelLoad < 0.3f)
    //        yield return new WaitForSeconds(.3f);
    //    
    //    PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound, gameObject));
    //
    //    float sqMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
    //    Vector3 targetPosOld = target.position;
    //
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(minPathUpdateTime);
    //        if ((target.position - targetPosOld).sqrMagnitude > sqMoveThreshold)
    //        {
    //            StopCoroutine("FollowPath");
    //            PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound, gameObject));
    //            targetPosOld = target.position;
    //        }
    //    }
    //}

    private void Update()
    {
        if (IsSelected && Input.GetMouseButtonDown(1))
        {
            SetMoveLocation();
        }
    }

    void SetMoveLocation()
    {
        /*
         * TO DO:
         * Add the functionality to pathfinding to search through different layermasks to allow different movement types
         * while at the same time increasing the speed by creating a neighbor bit check of booleans for each movement type
         * to incrase pathfinding speed.
         * 
         * Perhaps at level load detect all units that get loaded in and create this list for each unique movement type/layer combo
         * This could handle every case while also saving memory by not creating the neighbor list for non-existing combos
         * */
        RaycastHit hit;
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 512f, ValidMovementLayers))
        {
            StopCoroutine("FollowPath");
            PathRequestManager.RequestPath(new PathRequest(transform.position, hit.point, OnPathFound, gameObject, dimension));
        }
    }

    IEnumerator FollowPath()
    {
        //Vector3 currentWaypoint = path[0];

        //bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1f;

        /*To DO:
         * 1: Prevent units from moving past the next waypoint if they move too fast
         * 2: Calculate turn radius, then use radius to scan area of tiles to find obstacles.
         *      If there are obstacles, they need to stop and turn
         *      Otherwise, find some way to check if they've turned too far and need to move to the next point
         */
        while (true)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            /*float timeRemaining = Time.deltaTime;
            while (timeRemaining > 0.0f)
            {
                if (transform.position == path.lookPoints[pathIndex])
                {
                    ++pathIndex;
                    if (pathIndex < path.lookPoints.Length)
                    {
                        //float timeNeeded = (path.lookPoints[pathIndex] - transform.position).sqrMagnitude / (speed * speed);
                        //transform.position = Vector3.MoveTowards(transform.position, path.lookPoints[pathIndex], speed * timeRemaining);
                        Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                        transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

                        timeRemaining -= timeNeeded;
                    }
                    else
                    {
                        
                    }
                }

                
            }*/

            //while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            while ((transform.position - path.lookPoints[pathIndex]).sqrMagnitude < turnDist * turnDist)
            {
                if (pathIndex == path.finishLineIndex)
                {
                    //followingPath = false;
                    break;
                }
                else
                    ++pathIndex;
            }

            if (pathIndex >= path.slowDownIndex)
            {
                //float dist = path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D);
                float sqrDist = (transform.position - path.lookPoints[pathIndex]).sqrMagnitude;
                if (stoppingDist > 0)
                {
                    speedPercent = Mathf.Clamp(sqrDist / (stoppingDist * stoppingDist), 0.1f, 1f);
                }

                if (pathIndex == path.finishLineIndex && sqrDist <= 0.1f)
                {
                    transform.position = path.lookPoints[pathIndex];
                    break;
                }
            }

            Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

            yield return null;
        }
    }

    override public void Deselect()
    {
        SelectedEffect.SetActive(false);
        IsSelected = false;
    }

    override public void Select()
    {
        SelectedEffect.SetActive(true);
        IsSelected = true;
    }

    public override void SetHighlighted(bool IsHighlighted)
    {
        HighlightedEffect.SetActive(IsHighlighted);
    }

    public void OnDrawGizmos()
    {
        if (drawPath && path != null)
            path.DrawWithGizmos();
    }
}

static class MovementTypeLibrary
{
    //name of movement type and MovementType containing info about it
    static Dictionary<string, MovementType> TypesDict = new Dictionary<string, MovementType>();

    static public void Add(string newType)
    {

    }

    static public int GetMatchingModifier(string moveType, string terrainType)
    {
        MovementType type;
        TypesDict.TryGetValue(moveType, out type);
        return type.Get(terrainType);
    }

    static void Clear()
    {
        TypesDict.Clear();
    }
}

public class MovementType
{
    //Name of the terrain and its modifier
    Dictionary<string, int> TerrainSpeedModifiers = new Dictionary<string, int>();

    public void Add(string terrain, int modifier)
    {
        TerrainSpeedModifiers.Add(terrain, modifier);
    }

    public Dictionary<string, int> GetAll()
    {
        return TerrainSpeedModifiers;
    }

    public int Get (string Value)
    {
        int val;
        TerrainSpeedModifiers.TryGetValue(Value, out val);
        return val;
    }
}