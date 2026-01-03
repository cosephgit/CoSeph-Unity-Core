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
    public class CSParallax : MonoBehaviour
    {
        [SerializeField] private Transform follow;
        [Header("Each parallax layer and the virtual distance")]
        [SerializeField] private Transform[] layers;
        [SerializeField] private float[] distances;
        private Vector3 posOld;
        private int layerCount;
        private float[] layerScale;

        private void Awake()
        {
            if (!follow) follow = Camera.main.transform;
            posOld = new Vector3(follow.position.x, follow.position.y);
            layerCount = layers.Length;
            layerScale = new float[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                float dist = 1f;
                if (distances.Length > i) dist = distances[i];

                layerScale[i] = 1f - (1f / dist);
            }
        }

        private void Update()
        {
            if (follow.position.x != posOld.x || follow.position.y != posOld.y)
            {
                posOld.x = follow.position.x;
                posOld.y = follow.position.y;


                for (int i = 0; i < layerCount; i++)
                {
                    layers[i].transform.position = posOld * layerScale[i];
                }
            }
        }
    }
}