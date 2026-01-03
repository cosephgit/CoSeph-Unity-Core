using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavNodeEdge
// a simple class to attach to navigation barriers with a collider
// if CSNavigate finds one of these attached to a collider between two nodes, it will check if it is marked as blocked
// created 21/10/23
// changed 21/10/23

namespace CoSeph.Core
{
    public class CSNavNodeEdge : MonoBehaviour
    {
        public bool blocking = true;
    }
}