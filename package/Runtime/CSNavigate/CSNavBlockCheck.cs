using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavBlockCheck
// abstract class interface for CSNavigate to check if there is an obstacle at a node
// CSNavigate must have a reference to an implementation of this interface
// created 24/1/23
// modified 24/1/23

namespace CoSeph.Core
{
    public enum BlockType
    {
        Clear, // no blockage
        Pawn, // a pawn is blocking - might potentially move or be moveable
        Block // something impassable is blocking
    }


    public abstract class CSNavBlockCheck : MonoBehaviour
    {
        public abstract BlockType PointBlocked(Vector3 point, out float difficulty);
        public abstract BlockType NodeBlocked(CSNavNode node, out float difficulty);
    }
}