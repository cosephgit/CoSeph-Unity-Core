using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// CSNavigate checks for objects with colliders and this class, 
    /// so when they are placed between CSNavNode obstacles like walls can be identified.
    /// Extensible for custom edge behaviour.
    ///
    /// This component requires either a 3D Collider or a 2D Collider.
    /// The navigation system uses physics traces appropriate to the
    /// current navigation mode (2D or 3D).
    /// </summary>
    public class CSNavNodeEdge : MonoBehaviour
    {
        [SerializeField, Tooltip("If true this edge will block movement")]protected bool _blocking = true;

        protected virtual void Awake()
        {
            if (!HasCollider())
            {
                Debug.LogWarning("CSNavNodeEdge has no collider attached and will not function.");
                enabled = false;
            }
        }

        protected virtual bool HasCollider()
        {
            return TryGetComponent<Collider>(out _) || TryGetComponent<Collider2D>(out _);
        }

        // Check if this node is blocking movement, no default logic
        public virtual bool IsBlocking()
        {
            return _blocking;
        }
    }
}