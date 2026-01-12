using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    [System.Serializable]
    public class CSParallaxLayer
    {
        public Transform layer;
        public float distance; // Conceptual depth: higher = further away (slower parallax)
        [HideInInspector] public float scale; // Derived parallax factor (1 / distance)
        [HideInInspector] public Vector3 posStart; // World position as placed in the scene originally
        [HideInInspector] public Vector3 followOffsetCurrent; // Initial offset relative to current anchor

        public CSParallaxLayer(Transform layer, float distance)
        {
            this.layer = layer;
            this.distance = distance;
        }
    }

    /// <summary>
    /// Manages multiple parallax layers relative to a moving follow Transform,
    /// supporting axis locking, runtime follow reassignment, and optional anchoring.
    /// _followCam will set Camera.main as the target, overriding _follow
    ///
    /// Sample under Samples/Parallax/
    /// </summary>
    public class CSParallax : MonoBehaviour
    {
        [SerializeField] private Transform _follow;
        [SerializeField] private bool _followCam;
        [Tooltip("Interpret initial position relative to anchor instead of world origin")][SerializeField] private bool _followAnchor;
        [SerializeField] private CSParallaxLayer[] _layers; // Initial layer definitions validated and copied into _layersActive at runtime
        [Header("Axis locks - default to 2D")]
        [SerializeField] private bool _parallaxX = true;
        [SerializeField] private bool _parallaxY = true;
        [SerializeField] private bool _parallaxZ = false;
        private Vector3 _followStartPosition;
        private Vector3 _followPos; // store the last known position to check for changes
        // Runtime list of validated and initialised parallax layers
        private List<CSParallaxLayer> _layersActive = new List<CSParallaxLayer>();

        private void Awake()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i].layer != null
                    && _layers[i].distance > 0)
                    AddLayer(_layers[i]);
                else
                    Debug.LogWarning($"CSParallax.Awake _layers {i} is invalid.", this);
            }
            if (!_parallaxX && !_parallaxY && !_parallaxZ)
                Debug.LogWarning("CSParallax has all parallax axis disabled", this);
            if (_followCam)
            {
                if (Camera.main)
                    _follow = Camera.main.transform;
                else
                {
                    Debug.LogError("CSParallax followCam set but Camera.main is null", this);
                    enabled = false;
                    return;
                }
            }
            if (_follow == null)
            {
                Debug.LogError("CSParallax not set with target to follow - disabling", this);
                enabled = false;
                return;
            }
            InitFollow();
        }

        /// <summary>
        /// Add an additional layer to the parallax system.
        /// </summary>
        public void AddLayer(CSParallaxLayer layerAdd)
        {
            if (layerAdd != null)
            {
                if (layerAdd.layer != null)
                {
                    if (layerAdd.distance <= 0)
                    {
                        Debug.LogWarning("AddLayer " + layerAdd.layer + " has distance " + layerAdd.distance);
                    }
                    else
                    {
                        if (_layersActive.Contains(layerAdd))
                        {
                            Debug.LogWarning("_layersActive already contains " + layerAdd.layer);
                        }
                        else
                        {
                            // scale = parallax factor (1 = moves with follow, <1 = background, >1 = foreground)
                            layerAdd.scale = 1f - (1f / layerAdd.distance);
                            // Store the layer's initial world position as the current scene position
                            layerAdd.posStart = layerAdd.layer.position;
                            _layersActive.Add(layerAdd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add an additional layer from just the transform and distance
        /// </summary>
        public void AddLayer(Transform layer, float distance)
        {
            if (layer != null && distance > 0)
                AddLayer(new CSParallaxLayer(layer, distance));
        }

        private void InitFollow()
        {
            // Determine the reference point for parallax calculations:
            // - Anchored: parallax is relative to the follow's starting position
            // - Unanchored: parallax is relative to world origin (will jump to follower position immediately)
            if (_followAnchor)
                _followStartPosition = _follow.position;
            else
                _followStartPosition = Vector3.zero;

            // Cache each layer's initial offset relative to the follow reference point
            for (int i = 0; i < _layersActive.Count; i++)
                _layersActive[i].followOffsetCurrent = _layersActive[i].posStart - _followStartPosition;

            _followPos = Vector3.zero;
        }

        /// <summary>
        /// Allow setting a new follow target at runtime
        /// </summary>
        public void SetFollow(Transform followNew)
        {
            if (followNew)
            {
                _follow = followNew;
                InitFollow();
                enabled = true; // in case it was disabled during Awake()
            }
            else
            {
                Debug.LogWarning("CSParallax.SetFollow called with null followNew", this);
            }
        }

        public void StopFollow()
        {
            enabled = false;
        }

        private void LateUpdate()
        {
            if (_follow && _layersActive.Count > 0)
            {
                // Skip updates if the follow hasn't meaningfully moved
                if ((_follow.position - _followPos).sqrMagnitude > 0.0001f)
                {
                    _followPos = _follow.position;

                    Vector3 followDelta = _followPos - _followStartPosition;

                    Debug.Log("CSParallax new followDelta " + followDelta);

                    for (int i = 0; i < _layersActive.Count; i++)
                    {
                        if (_layersActive[i].layer != null) // in case it's deleted
                        {
                            Vector3 parallaxDelta = new Vector3(
                                _parallaxX ? followDelta.x * _layersActive[i].scale : 0f,
                                _parallaxY ? followDelta.y * _layersActive[i].scale : 0f,
                                _parallaxZ ? followDelta.z * _layersActive[i].scale : 0f);

                            _layersActive[i].layer.position = _followStartPosition + (_layersActive[i].followOffsetCurrent + parallaxDelta);
                        }
                    }
                }
            }
        }
    }
}