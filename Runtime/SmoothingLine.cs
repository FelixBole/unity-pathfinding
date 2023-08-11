using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slax.Pathfinding
{
    /// <summary>
    /// This class handles verification of if an Agent has passed a
    /// turn boundary
    /// </summary>
    public struct SmoothingLine
    {
        const float VerticalLineGradient = 1e5f;

        float _gradient;
        float _yIntercept;
        float _gradientPerpendicular;

        Vector2 _pointOnLine1;
        Vector2 _pointOnLine2;

        bool _approachSide;

        public SmoothingLine(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float dx = pointOnLine.x - pointPerpendicularToLine.x;
            float dy = pointOnLine.y - pointPerpendicularToLine.y;

            _gradientPerpendicular = dx == 0 ? VerticalLineGradient : dy / dx;
            _gradient = _gradientPerpendicular == 0 ? VerticalLineGradient : -1 / _gradientPerpendicular;

            _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;
            _pointOnLine1 = pointOnLine;
            _pointOnLine2 = pointOnLine + new Vector2(1, _gradient);

            _approachSide = false;
            _approachSide = GetSide(pointPerpendicularToLine);
        }

        bool GetSide(Vector2 point)
        {
            return (point.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y) > (point.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.x);
        }

        public bool HasCrossedLine(Vector2 p) => GetSide(p) != _approachSide;

        public float DistanceFromPoint(Vector2 p)
        {
            float yInterceptPerpendicular = p.y - _gradientPerpendicular * p.x;
            float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
            float intersectY = _gradient * intersectX + _yIntercept;
            return Vector2.Distance(p, new Vector2(intersectX, intersectY));
        }

        public void DrawWithGizmos(float length, bool is3D = false)
        {
            Vector3 lineDir = new Vector3(1, is3D ? 0 : _gradient, is3D ? _gradient : 0).normalized;
            Vector3 lineCenter = new Vector3(_pointOnLine1.x, is3D ? 0 : _pointOnLine1.y, is3D ? _pointOnLine1.y : 0) + (is3D ? Vector3.up : -Vector3.forward);
            Vector3 lineFrom = lineCenter - lineDir * length / 2f;
            Vector3 lineTo = lineCenter + lineDir * length / 2f;
            Gizmos.DrawLine(lineFrom, lineTo);
        }
    }
}
