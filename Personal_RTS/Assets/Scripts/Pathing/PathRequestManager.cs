﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

namespace Pathing
{

    public class PathRequestManager : MonoBehaviour
    {
        List<PathRequest> pathRequestQueue = new List<PathRequest>();
        //PathRequest currentPathRequest;
        Queue<PathResult> results = new Queue<PathResult>();

        static PathRequestManager instance;
        //Pathfinding pathfinding;

        bool isProcessingPath;

        private void Awake()
        {
            instance = this;
            //pathfinding = GetComponent<Pathfinding>();
        }

        public static void RequestPath(PathRequest request)
        {
            //Debug.Log("Adding path request for: " + request.requester.name);
            instance.RemoveSameUnitRequest(request);
            instance.pathRequestQueue.Add(request);
            instance.TryProcessNext();
        }

        bool RemoveSameUnitRequest(PathRequest newRequest)
        {
            //lock(pathRequestQueue)
            //{
            //    for (int i = 0; i < pathRequestQueue.Count; ++i)
            //    {
            //        if (pathRequestQueue[i].requester != newRequest.requester)
            //            continue;
            //
            //        PathRequest p = pathRequestQueue[i];
            //
            //        pathRequestQueue.RemoveAt(i);
            //
            //        pathRequestQueue.Add(newRequest);
            //
            //        //p.pathStart = newRequest.pathStart;
            //        //p.pathEnd = newRequest.pathEnd;
            //
            //        return true;
            //    }
            //    return false;
            //}

            return false;
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
            if (results.Count > 0)
            {
                PathResult result = results.Dequeue();
                if (!result.Success)
                {
                    Debug.Log("Pathing failed due to: " + result.failReason);
                    //continue;
                }
                result.callback(result.Path, result.Success);
            }

            instance.TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                isProcessingPath = true;
                Thread thread = new Thread(delegate ()
                {
                    PathRequest p = pathRequestQueue[0];
                    pathRequestQueue.RemoveAt(0);
                //instance.pathfinding.FindPath(p, FinishedProcessingPath);
            });
                thread.Start();
            }
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
        public Vector3[] Path;
        public bool Success;
        public Action<Vector3[], bool> callback;
        public string failReason;

        public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback, string _failReason)
        {
            Path = _path;
            Success = _success;
            callback = _callback;
            failReason = _failReason;
        }
    }

    public struct PathRequest
    {
        public Vector3 pathStart, pathEnd;
        public Action<Vector3[], bool> callback;
        public GameObject requester;
        public LayerMask dimension;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, GameObject _requester, LayerMask _dimension)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            requester = _requester;
            dimension = _dimension;
        }
    }

}