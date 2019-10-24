using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{/*
    Grid grid;

    //Heap<Node> open = new Heap<Node>(0);
    //HashSet<Node> closed = new HashSet<Node>();

    uint pathID = uint.MaxValue;

    double totalTime = 0;

    private void Awake()
    {
        //grid = GetComponent<Grid>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        //grid = DimensionManager.GetGridOfDimension(request.dimension);
        grid = Grid.GetGrid;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        string failReason = "Success";

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        if (startNode.GetWalkableByDimension(request.dimension) && targetNode.GetWalkableByDimension(request.dimension) && startNode != targetNode)
        {
            ++pathID;

            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            //HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                currentNode.lastPathID = pathID;
                //closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    //print("Path Found: " + sw.ElapsedMilliseconds + " ms");
                    totalTime += sw.ElapsedMilliseconds;
                    if (pathID > 0 && pathID % 100 == 0)
                    {
                        print("Average Time: " + (totalTime / pathID) + " ms");
                    }
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbor in grid.GetNeighbors(request.dimension, currentNode))
                {
                    if (!neighbor.GetWalkableByDimension(request.dimension) || neighbor.lastPathID == pathID)
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                        else
                            openSet.UpdateItem(neighbor);
                    }
                }
            }
            if (openSet.Count <= 0)
            {
                failReason = "openSet is empty";
            }

            //lock(open)
            //{
            //    lock (closed)
            //    {
            //        if (pathSuccess)
            //        {
            //            open = openSet;
            //            closed = closedSet;
            //        }
            //    }
            //}
        }
        else
        {
            if (!startNode.GetWalkableByDimension(request.dimension))
            {
                failReason = "Start Location isn't walkable in dimension: " + (Mathf.Log(request.dimension >> 7, 2)) + " at grid: " + startNode.gridX + ", " + startNode.gridY;
            }
            if (!targetNode.GetWalkableByDimension(request.dimension))
            {
                failReason = "Target Location isn't walkable in dimension: " + (Mathf.Log(request.dimension >> 7, 2)) + " at grid: " + startNode.gridX + ", " + startNode.gridY;
            }
            if (startNode == targetNode)
            {
                failReason = "Start location (" + request.pathStart + ") and target location (" + request.pathEnd + ") are the same node";
            }
        }
        
        if(pathSuccess)
            waypoints = RetracePath(startNode, targetNode);
        callback(new PathResult(waypoints, pathSuccess, request.callback, failReason));
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }
    
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        waypoints.Add(path[0].worldPosition);

        for (int curr = 1; curr < path.Count; ++curr)
        {
            ////If not a corner
            //if (!path[curr].isCorner)
            //{
            //    //Get the direction from the previous turn to the current node
            //    Vector2 directionNew = new Vector2(path[curr - 1].gridX - path[curr].gridX, path[curr -1].gridY - path[curr].gridY);
            //
            //    //If the direction changed, update the previous turn and add the node to the waypoints
            //    if (directionNew != directionOld)
            //    {
            //        waypoints.Add(path[curr].worldPosition);
            //    }
            //
            //    directionOld = directionNew;
            //
            //    continue;
            //}

            waypoints.Add(path[curr].worldPosition);
        }

        if (waypoints.Count <= 1)
        {
            grid.debugPath = path;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    //private void OnDrawGizmos()
    //{
    //    open.ToArray();
    //    foreach (Node n in open.ToArray())
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.5f);
    //    }
    //
    //    foreach (Node n in closed)
    //    {
    //        Gizmos.color = Color.black;
    //        Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.5f);
    //    }
    //}*/
}
