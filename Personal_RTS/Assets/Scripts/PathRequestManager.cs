using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    //PathRequest currentPathRequest;
    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(PathRequest request)//Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        //ThreadStart threadStart = delegate
        //{
        //    instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        //};
        //threadStart.Invoke();
        instance.RemoveSameUnitRequest(request);
        instance.pathRequestQueue.Enqueue(request);
        instance.TryProcessNext();
    }

    void RemoveSameUnitRequest(PathRequest newRequest)
    {
        //lock(pathRequestQueue)
        //{
        //    for(int i = 0; i < pathRequestQueue.Count; ++i)
        //    {
        //        foreach (PathRequest request in pathRequestQueue)
        //        {
        //            if(request.requester == newRequest.requester)
        //            {
        //                request = newRequest;
        //            }
        //        }
        //    }
        //    for (int i = 0; i < pathRequestQueue.Count; ++i)
        //    {
        //
        //    }
        //}
    }

    private void Update()
    {
        //if (results.Count > 0)
        //{
        //    int resultsInQueue = results.Count;
        //    lock(results)
        //    {
        //        for(int i = 0; i < resultsInQueue; ++i)
        //        {
        //            PathResult result = results.Dequeue();
        //            result.callback(result.path, result.success);
        //        }
        //    }
        //}
        while(results.Count > 0)
        {
            PathResult result = results.Dequeue();
            result.callback(result.path, result.success);
        }
    }

    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            isProcessingPath = true;
            Thread thread = new Thread(delegate ()
            {
                instance.pathfinding.FindPath(pathRequestQueue.Dequeue(), FinishedProcessingPath);
            });
            thread.Start();
        }
        //if (!isProcessingPath && pathRequestQueue.Count > 0)
        //{
        //    currentPathRequest = pathRequestQueue.Dequeue();
        //    isProcessingPath = true;
        //    pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        //}
    }

    public void FinishedProcessingPath(PathResult result)
    {
        //lock (results)
        //{
            results.Enqueue(result);
        //}
        //result.callback(result.path, result.success);
        isProcessingPath = false;
        TryProcessNext();
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback)
    {
        path = _path;
        success = _success;
        callback = _callback;
    }
}

public struct PathRequest
{
    public Vector3 pathStart, pathEnd;
    public Action<Vector3[], bool> callback;
    public GameObject requester;

    public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, GameObject _requester)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
        requester = _requester;
    }
}
