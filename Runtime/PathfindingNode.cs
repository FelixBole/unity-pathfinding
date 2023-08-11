using UnityEngine;

namespace Slax.Pathfinding
{
    public class PathfindingNode : IHeapItem<PathfindingNode>
    {
        public bool Walkable;
        public Vector3 WorldPosition;
        public int GridX;
        public int GridY;
        /// <summary>Used for evaluating weights in the path</summary>
        public int MovementPenalty;

        /// <summary>Distance from starting node</summary>
        public int GCost;
        /// <summary>Distance from target node</summary>
        public int HCost;
        public int FCost => GCost + HCost;

        public PathfindingNode ParentNode;

        private int _heapIndex;

        public PathfindingNode(bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty)
        {
            Walkable = walkable;
            WorldPosition = worldPos;
            GridX = gridX;
            GridY = gridY;
            MovementPenalty = penalty;
        }

        public int HeapIndex
        {
            get { return _heapIndex; }
            set { _heapIndex = value; }
        }

        public int CompareTo(PathfindingNode nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);

            if (compare == 0)
            {
                compare = HCost.CompareTo(nodeToCompare.HCost);
            }

            return -compare;
        }

    }
}