using UnityEngine;

namespace Slax.Pathfinding
{
    public class PathfindingPath
    {
        public readonly Vector3[] LookPoints;
        public readonly SmoothingLine[] TurnBoundaries;
        public readonly int FinishLineIndex;
        public readonly int SlowDownIndex;

        private bool _is3D;

        public PathfindingPath(Vector3[] waypoints, Vector3 startPos, float turnDistance, bool is3D, float stoppingDst)
        {
            _is3D = is3D;
            LookPoints = waypoints;
            TurnBoundaries = new SmoothingLine[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            Vector2 previousPoint = is3D ? V3ToV2(startPos) : (Vector2)startPos;
            for (int i = 0; i < LookPoints.Length; i++)
            {
                Vector2 currentPoint = is3D ? V3ToV2(LookPoints[i]) : (Vector2)LookPoints[i];
                Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                Vector2 turnBoundaryPoint = (i == FinishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDistance;
                TurnBoundaries[i] = new SmoothingLine(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDistance);
                previousPoint = turnBoundaryPoint;
            }

            float dstFromEndPoint = 0;
            for (int i = LookPoints.Length - 1; i > 0; i--)
            {
                dstFromEndPoint += Vector3.Distance(LookPoints[i], LookPoints[i - 1]);
                if (dstFromEndPoint > stoppingDst)
                {
                    SlowDownIndex = i;
                    break;
                }
            }
        }

        Vector2 V3ToV2(Vector3 v3) => new Vector2(v3.x, v3.z);

        public void DrawWithGizmos(Color point, Color smoothLine)
        {
            Gizmos.color = point;
            foreach (Vector3 p in LookPoints)
            {
                Vector3 startPos = _is3D ? p + Vector3.up : p - Vector3.forward;
                Gizmos.DrawCube(startPos, Vector3.one);
            }

            Gizmos.color = smoothLine;
            foreach (var l in TurnBoundaries)
            {
                l.DrawWithGizmos(10, _is3D);
            }
        }
    }
}
