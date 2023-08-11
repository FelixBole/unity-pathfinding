using UnityEngine;
using UnityEngine.Events;

namespace Slax.Pathfinding
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool, GameObject> { }

    [System.Serializable]
    public class GameObjectEvent : UnityEvent<GameObject> { }

    public class PathfindingTriggerZone : MonoBehaviour
    {

        [SerializeField] private BoolEvent _enterZone = default;
        [SerializeField] private GameObjectEvent _stayZone = default;
        [SerializeField] private LayerMask _layers = default;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((1 << other.gameObject.layer & _layers) != 0)
            {
                _enterZone.Invoke(true, other.gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if ((1 << other.gameObject.layer & _layers) != 0)
            {
                _stayZone.Invoke(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if ((1 << other.gameObject.layer & _layers) != 0)
            {
                _enterZone.Invoke(false, other.gameObject);
            }
        }
    }
}

