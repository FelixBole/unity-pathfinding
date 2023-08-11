using UnityEngine;

namespace Slax.Pathfinding
{
    [CreateAssetMenu(menuName = "Slax/Pathfinding/Base/Agent Configuration", fileName = "NewAgentConfiguration")]
    public class PathfindingAgentConfigurationBase : ScriptableObject
    {
        /// <summary>Whether or not we're working in 3D space</summary>
        public bool Is3D = false;
        /// <summary>Whether or not to use path smoothing logic</summary>
        public bool UsePathSmoothing = true;
        /// <summary>Whether or not to slow down when reaching the target</summary>
        public bool UseSlowDownOnTargetReach = false;
        /// <summary>Allows A* to look for diagonal nodes</summary>
        public bool AllowDiagonalMovement = true;
        /// <summary>Target's move distance threshold before allowing to call a path update</summary>
        public float PathUpdateMoveThreshold = .5f;
        /// <summary>Minimum amount of time before two Path updates</summary>
        public float MinPathUpdateTime = .2f;
        /// <summary>Agent move speed</summary>
        public float MoveSpeed = 10f;
        /// <summary>Agent move speed</summary>
        public float TurnSpeed = 3f;
        /// <summary>Distance from a smoothline the agent will start turning to the next path point</summary>
        public float TurnDistance = 5f;
        /// <summary>Distance from the target at which the Agent start to slow down</summary>
        public float StoppingDistance = 10f;
        /// <summary>Speed at which the Agent is considered to have reached the end and will stop</summary>
        public float MinimumStoppingDistanceSpeed = .01f;


        // --- DEBUG

        public bool ShowAllAgentsPathGizmos = false;
    }

}