using System.Collections;
using UnityEngine;

namespace Slax.Pathfinding
{
    public class PathfindingAgent : MonoBehaviour
    {
        public Transform Target;
        public PathfindingAgentConfigurationBase Configuration;
        PathfindingPath _path;

        [Header("Debug")]
        [SerializeField] private bool _showPathGizmos = false;
        [SerializeField] private Color _pathGizmosPoint = Color.black;
        [SerializeField] private Color _pathGizmosSmoothLine = Color.white;

        void Start()
        {
            StartCoroutine(UpdatePath());
        }

        public void OnPathFound(Vector3[] waypoints, bool pathSuccess)
        {
            if (pathSuccess)
            {
                _path = new PathfindingPath(waypoints, transform.position, Configuration.TurnDistance, Configuration.Is3D, Configuration.StoppingDistance);

                if (Configuration.UsePathSmoothing)
                {
                    Debug.Log("Restarting FollowPath Coroutine with new path");
                    StopCoroutine(FollowPath());
                    StartCoroutine(FollowPath());
                }
                else
                {
                    StopCoroutine(FollowRawPath());
                    StartCoroutine(FollowRawPath());
                }

            }
        }
        IEnumerator FollowPath()
        {
            bool followingPath = true;
            int pathIndex = 0;
            float speedPercent = 1;

            Debug.Log("Starting new follow path");

            if (Configuration.Is3D)
            {
                transform.LookAt(_path.LookPoints[0]);
            }
            // else
            // {
            //     Vector3 targetPos = _path.LookPoints.Length != 0 ? _path.LookPoints[0] : transform.position;
            //     Vector3 relativePos = targetPos - transform.position;
            //     float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;

            //     Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            //     transform.rotation = targetRotation;
            // }


            while (followingPath)
            {
                Vector2 pos2D = Configuration.Is3D ? new Vector2(transform.position.x, transform.position.z) : (Vector2)transform.position;

                // While here because if it goes too fast it can pass multiple SmoothLines
                // in a single frame causing undesired behaviours
                while (pathIndex < _path.TurnBoundaries.Length && _path.TurnBoundaries[pathIndex].HasCrossedLine(pos2D))
                {
                    if (pathIndex == _path.FinishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }


                if (pathIndex < _path.LookPoints.Length && followingPath)
                {
                    if (Configuration.UseSlowDownOnTargetReach && pathIndex >= _path.SlowDownIndex && Configuration.StoppingDistance > 0)
                    {
                        speedPercent = Mathf.Clamp01(_path.TurnBoundaries[_path.FinishLineIndex].DistanceFromPoint(pos2D) / Configuration.StoppingDistance);
                        if (speedPercent < Configuration.MinimumStoppingDistanceSpeed)
                        {
                            followingPath = false;
                        }
                    }

                    if (Configuration.Is3D)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(_path.LookPoints[pathIndex] - transform.position);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * Configuration.TurnSpeed);

                        // Space.Self moving in relation to its own rotation
                        transform.Translate(Vector3.forward * Time.deltaTime * Configuration.MoveSpeed * speedPercent, Space.Self);
                    }
                    else
                    {
                        Vector3 relativePos = _path.LookPoints[pathIndex] - transform.position;
                        float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;

                        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * Configuration.TurnSpeed);

                        transform.Translate(Vector3.right * Time.deltaTime * Configuration.MoveSpeed * speedPercent, Space.Self);
                    }
                }

                yield return null;
            }
        }

        IEnumerator UpdatePath()
        {
            // Handling long wait times on level load / editor start
            if (Time.timeSinceLevelLoad < .3f)
            {
                yield return new WaitForSeconds(.3f);
            }

            PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);

            // Comparing square distances is faster than comparing distances
            float sqrMoveThreshold = Configuration.PathUpdateMoveThreshold * Configuration.PathUpdateMoveThreshold;
            Vector3 targetPosOld = Target.position;

            while (true)
            {
                yield return new WaitForSeconds(Configuration.MinPathUpdateTime);
                if ((Target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
                    targetPosOld = Target.position;
                }
            }
        }

        IEnumerator FollowRawPath()
        {
            bool followingPath = true;
            int pathIndex = 0;
            Vector3 currentWaypoint = _path.LookPoints[0];

            while (followingPath)
            {
                if (transform.position == currentWaypoint)
                {
                    // Advance to next waypoint
                    pathIndex++;
                    if (pathIndex >= _path.LookPoints.Length)
                    {
                        yield break;
                    }
                    currentWaypoint = _path.LookPoints[pathIndex];
                }

                Vector3 relativePos = Target.position - transform.position;
                float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, Configuration.MoveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        void OnDrawGizmos()
        {
            if (_path != null && (Configuration.ShowAllAgentsPathGizmos || _showPathGizmos))
            {
                _path.DrawWithGizmos(_pathGizmosPoint, _pathGizmosSmoothLine);
            }
        }
    }
}
