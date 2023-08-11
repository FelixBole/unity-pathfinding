using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Slax.Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        [SerializeField] private PathfindingGridDataSO _data;
        public int MaxSize => _gridSizeX * _gridSizeY;

        protected Pathfinding _pathfinding;
        protected Vector3 _worldBottomLeft;
        protected Vector3 _worldCenter;
        protected Vector2 _gridWorldSize;
        protected PathfindingNode[,] _grid;
        protected float _nodeDiameter;
        protected int _gridSizeX, _gridSizeY;
        protected int _penaltyMax = int.MinValue;
        protected int _penaltyMin = int.MaxValue;

        protected LayerMask _walkableMask;
        protected Dictionary<int, int> _walkableRegionsDict = new Dictionary<int, int>();

        protected Tilemap _tilemap = null;

        protected virtual void Awake()
        {
            _pathfinding = GetComponent<Pathfinding>();
            Init(transform.position, _pathfinding);
        }

        public PathfindingGrid Init(Vector3 worldCenter, Pathfinding pathfinding)
        {
            BaseInit(worldCenter, _data.GridSize, pathfinding);
            return this;
        }

        public PathfindingGrid Init(Vector3 worldCenter, Vector2 gridSize, Pathfinding pathfinding)
        {
            BaseInit(worldCenter, gridSize, pathfinding);
            return this;
        }

        public PathfindingGrid Init(Vector3 worldCenter, Vector2 gridSize, Pathfinding pathfinding, Tilemap tilemap)
        {
            BaseInit(worldCenter, gridSize, pathfinding);
            return this;
        }

        protected void BaseInit(Vector3 worldCenter, Vector2 gridSize, Pathfinding pathfinding)
        {
            _worldCenter = worldCenter;
            _gridWorldSize = gridSize;

            _nodeDiameter = _data.NodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);

            foreach (TerrainType region in _data.WalbakbleRegions)
            {
                // bitwise OR, ref: https://youtu.be/T0Qv4-KkAUo?t=517
                _walkableMask.value |= region.TerrainMask.value;
                _walkableRegionsDict.Add((int)Mathf.Log(region.TerrainMask.value, 2), region.TerrainPenalty);
            }

            CreateGrid();
            _pathfinding = pathfinding;
        }

        protected void CreateGrid()
        {
            _grid = new PathfindingNode[_gridSizeX, _gridSizeY];
            _worldBottomLeft = _worldCenter - Vector3.right * _gridWorldSize.x / 2 - (_data.Is3D ? Vector3.forward : Vector3.up) * _gridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = _worldBottomLeft + Vector3.right * (x * _nodeDiameter + _data.NodeRadius) + (_data.Is3D ? Vector3.forward : Vector3.up) * (y * _nodeDiameter + _data.NodeRadius);
                    bool walkable = !(Physics2D.OverlapCircle(worldPoint, _data.NodeRadius - _data.ObstacleCheckDistance, _data.ObstacleLayer));

                    int movementPenalty = 0;

                    if (_data.Is3D)
                    {
                        Ray ray = new Ray(worldPoint + Vector3.up * _data.WeightDistanceCheck, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, _data.WeightDistanceCheck * 2, _walkableMask))
                        {
                            _walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }
                    }
                    else
                    {
                        float heightCheck = 10f;
                        RaycastHit2D hit2D = Physics2D.Raycast(worldPoint - (Vector3.forward * heightCheck), Vector3.forward, _data.WeightDistanceCheck, _walkableMask);
                        if (hit2D.collider != null)
                        {
                            _walkableRegionsDict.TryGetValue(hit2D.collider.gameObject.layer, out movementPenalty);
                        }
                    }

                    if (!walkable) movementPenalty += _data.ObstacleProximityPenalty;

                    _grid[x, y] = new PathfindingNode(walkable, worldPoint, x, y, movementPenalty);
                }
            }

            if (_data.UseWeightBlurSmoothing)
            {
                BlurPenaltyMap(_data.WeigthSmoothingBlurSize);
            }
        }

        void BlurPenaltyMap(int blurSize)
        {
            // Has to be odd to have a central square
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
                }

                for (int x = 1; x < _gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - _grid[removeIndex, y].MovementPenalty + _grid[addIndex, y].MovementPenalty;
                }
            }


            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                _grid[x, 0].MovementPenalty = blurredPenalty;

                for (int y = 1; y < _gridSizeY; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    _grid[x, y].MovementPenalty = blurredPenalty;

                    if (blurredPenalty > _penaltyMax)
                        _penaltyMax = blurredPenalty;

                    if (blurredPenalty < _penaltyMin)
                        _penaltyMin = blurredPenalty;
                }
            }
        }

        public List<PathfindingNode> GetNeighbors(PathfindingNode node)
        {
            List<PathfindingNode> neighbors = new List<PathfindingNode>();

            // Search in 3 by 3 block
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        neighbors.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }

        public PathfindingNode NodeFromWorldPoint(Vector3 worldPosition)
        {
            if (_tilemap != null)
            {
                Vector3Int pos = _tilemap.WorldToCell(worldPosition);

                return _grid[pos.x, pos.y];
            }

            Vector3 localPos = worldPosition - _worldCenter; // Convert to local space
            float percentX = (localPos.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
            float zPos = _data.Is3D ? localPos.z : localPos.y;
            float percentY = (zPos + _gridWorldSize.y / 2) / _gridWorldSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];

        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(_data.GridSize.x, _data.Is3D ? 1 : _data.GridSize.y, _data.Is3D ? _data.GridSize.y : 1));
            Gizmos.color = Color.gray;
            if (_grid != null && _data.ShowNodeGizmos)
            {
                foreach (PathfindingNode node in _grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(_penaltyMin, _penaltyMax, node.MovementPenalty));
                    Gizmos.color = node.Walkable ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(node.WorldPosition, (_data.Is3D ? Vector3.up : Vector3.one) * _nodeDiameter);
                }
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        // Todo write custom editor to make it so that
        // each mask has a single layer selected
        public LayerMask TerrainMask;

        /// <summary>
        /// Strength of "avoidance" of a certain terrain
        /// </summary>
        [Tooltip("Higher values will decrease likeliness of chosing Path with this LayerMask")]
        public int TerrainPenalty;
    }

}
