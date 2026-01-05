using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSParallax
// manages moving parallax layers w.r.t. the provided Transform
// defaults to Camera.Main if no transform is provided
// created 1/2/24
// modified 1/2/24

namespace CoSeph.Core
{
    [System.Serializable]
    public class CSParallaxLayer
    {
        public Transform _layer;
        public float _distance;
        [HideInInspector]public float _scale;
    }

    public class CSParallax : MonoBehaviour
    {
        [SerializeField] private Transform _follow;
        [SerializeField] private bool _followCam;
        [SerializeField] private CSParallaxLayer[] _layers;
        private Vector3 _posOld;
        private int _layerCount;

        private void Awake()
        {
            if (_followCam)
            {
                _follow = Camera.main.transform;
            }
            _posOld = new Vector3(_follow.position.x, _follow.position.y);
            _layerCount = _layers.Length;
            for (int i = 0; i < _layerCount; i++)
            {
                float dist = 1f;
                if (_layers[i]._distance > i) dist = _layers[i]._distance;

                _layers[i]._scale = 1f - (1f / dist);
            }
        }

        private void Update()
        {
            if (_follow)
            {
                if (_follow.position.x != _posOld.x || _follow.position.y != _posOld.y)
                {
                    _posOld.x = _follow.position.x;
                    _posOld.y = _follow.position.y;


                    for (int i = 0; i < _layerCount; i++)
                    {
                        _layers[i]._layer.position = _posOld * _layers[i]._scale;
                    }
                }
            }
        }
    }
}