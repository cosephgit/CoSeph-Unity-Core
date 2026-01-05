using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavProfile
// part of the CSNavigate bundle
// this class is used to handle tailored navigation parameters e.g. custom checks for obstacles, path prioritising and move costs
// created 23/2/24
// modified 23/2/24

namespace CoSeph.Core
{
    public class CSNavProfile
    {
        public NavArb _arb = NavArb.Simple; // how to arbitrate between points of tied value
        public BlockHandling _block = BlockHandling.Avoid; // how to treat blockages
        public float _max = -1; // the maximum path distance
        public float _divertMultOverride = -1; // multiplier to maximum divert distance that will be considered (if choosing a longer diversion around a blockage)
        private int _layersAvoid = -1;

        public CSNavProfile(NavArb arb, BlockHandling block, float max, float divertMultOverride, int layersAvoid = -1)
        {
            _arb = arb;
            _block = block;
            _max = max;
            _divertMultOverride = divertMultOverride;
            _layersAvoid = layersAvoid;
        }

        // check if the provided node is passable at all
        public BlockType CheckBlocked(Vector3 point, bool tryMove)
        {
            CSNavNode pointNode = CSNavigate.Instance.GetNode(point);
            if (pointNode)
                return CheckBlocked(CSNavigate.Instance.GetNode(point), tryMove);
            return BlockType.Block;
        }
        public virtual BlockType CheckBlocked(CSNavNode node, bool tryMove)
        {
            if (_layersAvoid > 0)
            {
                Collider2D blocked = Physics2D.OverlapPoint(node.transform.position, _layersAvoid);
                if (blocked)
                    return BlockType.Block;
            }
            return BlockType.Clear;
        }

        // check for the movement cost of moving into this node
        public virtual float MoveCost(CSNavNode node)
        {
            return 1f;
        }

        // check for the weight this node should be given for path finding
        // HIGH weight means AVOID (e.g. to avoid moving through dangerous spaces unless necessary)
        public virtual float MoveWeight(CSNavNode node)
        {
            return 1f;
        }
    }
}