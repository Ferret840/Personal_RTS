using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Owner
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public float speed = 5;
    public float turnDist = 5;
    public float turnSpeed = 3;
    public float stoppingDist = 10;

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
            PathRequestManager.RequestPath(new PathRequest(transform.position, hit.point, OnPathFound, gameObject));
        }
    }

    IEnumerator FollowPath()
    {
        //Vector3 currentWaypoint = path[0];

        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1f;

        while (true)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    ++pathIndex;
            }

            if(followingPath)
            {
                if (pathIndex >= path.slowDownIndex)
                {
                    float dist = path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D);
                    if (stoppingDist > 0)
                    {
                        speedPercent = Mathf.Clamp(dist / stoppingDist, 0.1f, 1f);
                    }

                    if (pathIndex == path.finishLineIndex && dist <= 0.1f)
                    {
                        transform.position = path.lookPoints[pathIndex];
                        break;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            /*float timeRemaining = Time.deltaTime;
            while (timeRemaining > 0.0f)
            {
                if (transform.position == currentWaypoint)
                {
                    ++targetIndex;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }

                    currentWaypoint = path[targetIndex];
                }

                float timeNeeded = (currentWaypoint - transform.position).magnitude / speed;
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * timeRemaining);

                timeRemaining -= timeNeeded;
            }*/

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
        //if (path != null)
        //    path.DrawWithGizmos();
    }
}
