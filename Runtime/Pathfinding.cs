using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slax.Pathfinding
{
    [RequireComponent(typeof(PathRequestManager))]
    [RequireComponent(typeof(PathfindingGrid))] // Todo find a way to remove this dep
    public class Pathfinding : MonoBehaviour
    {
        PathRequestManager _requestManager;
        PathfindingGrid _grid;

        void Awake()
        {
            _requestManager = GetComponent<PathRequestManager>();
            _grid = GetComponent<PathfindingGrid>();
        }

        public void StartPathfind(Vector3 startPos, Vector3 targetPos)
        {
            StartCoroutine(FindPath(startPos, targetPos));
        }

        IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
        {

            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;

            PathfindingNode startNode = _grid.NodeFromWorldPoint(startPos);
            PathfindingNode targetNode = _grid.NodeFromWorldPoint(targetPos);

            if (startNode.Walkable && targetNode.Walkable)
            {
                Heap<PathfindingNode> openSet = new Heap<PathfindingNode>(_grid.MaxSize);
                HashSet<PathfindingNode> closedSet = new HashSet<PathfindingNode>();

                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    // Find node in openset with lowest FCost
                    PathfindingNode currentNode = openSet.RemoveFirst();

                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        // RetracePath(startNode, targetNode);
                        pathSuccess = true;
                        break;
                    }

                    foreach (PathfindingNode neighbour in _grid.GetNeighbors(currentNode))
                    {
                        if (!neighbour.Walkable || closedSet.Contains(neighbour))
                            continue;

                        int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;

                        if (newCostToNeighbor < neighbour.GCost || !openSet.Contains(neighbour))
                        {
                            neighbour.GCost = newCostToNeighbor;
                            neighbour.HCost = GetDistance(neighbour, targetNode);
                            neighbour.ParentNode = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                            else
                                openSet.UpdateItem(neighbour);
                        }
                    }
                }

            }
            yield return null;
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
            }
            _requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        }

        private Vector3[] RetracePath(PathfindingNode startNode, PathfindingNode endNode)
        {
            List<PathfindingNode> path = new List<PathfindingNode>();
            PathfindingNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.ParentNode;
            }

            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }

        private Vector3[] SimplifyPath(List<PathfindingNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 oldDirection = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                // Get direction from current node to next node
                Vector2 newDirection = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);

                if (newDirection != oldDirection)
                {
                    waypoints.Add(path[i].WorldPosition);
                }

                oldDirection = newDirection;
            }

            return waypoints.ToArray();
        }

        private int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
        {
            int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            if (distX > distY)
                return (14 * distY) + 10 * (distX - distY);
            return (14 * distX) + 10 * (distY - distX);
        }
    }
}