using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Pathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        private Queue<PathRequest> PathRequestQueue = new Queue<PathRequest>();
        private PathRequest _currentPathRequest;

        static PathRequestManager _instance;
        Pathfinding _pathfinding;

        bool _isProcessingPath;

        void Awake()
        {
            _instance = this;
            _pathfinding = GetComponent<Pathfinding>();
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, UnityAction<Vector3[], bool> callback)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
            _instance.PathRequestQueue.Enqueue(newRequest);
            _instance.TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!_isProcessingPath && PathRequestQueue.Count > 0)
            {
                _currentPathRequest = PathRequestQueue.Dequeue();
                _isProcessingPath = true;
                _pathfinding.StartPathfind(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            _currentPathRequest.Callback(path, success);
            _isProcessingPath = false;
            TryProcessNext();
        }

        public struct PathRequest
        {
            public Vector3 PathStart;
            public Vector3 PathEnd;
            public UnityAction<Vector3[], bool> Callback;

            public PathRequest(Vector3 start, Vector3 end, UnityAction<Vector3[], bool> callback)
            {
                PathStart = start;
                PathEnd = end;
                Callback = callback;
            }
        }
    }
}
