using System.Collections.Generic;
using UnityEngine;

namespace Slax.Pathfinding
{

    [CreateAssetMenu(menuName = "Slax/Pathfinding/Grid Data", fileName = "PathfindingGridData")]
    public class PathfindingGridDataSO : ScriptableObject
    {
        public Vector2 GridSize = new Vector2();
        public float NodeRadius = .5f;
        public float ObstacleCheckDistance = .1f;
        public float WeightDistanceCheck = 50f;
        public int WeigthSmoothingBlurSize = 3;
        public int ObstacleProximityPenalty = 5;
        public bool UseWeightBlurSmoothing = false;
        public bool Is3D = false;
        public LayerMask ObstacleLayer;
        /// <summary>
        /// In 2D space, layers need to have an order of priority on their
        /// transform Z coordinate.
        ///
        /// For example, a road sitting on grass, should have a lower Z
        /// transform value than the grass for it to be considered
        /// properly
        /// </summary>
        public TerrainType[] WalbakbleRegions;

        [Header("Debug")]
        public bool ShowNodeGizmos = false;
    }

}