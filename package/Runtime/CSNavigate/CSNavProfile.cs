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
        public CSNavBlockRules BlockRules { get; }
        public NavArb _arb = NavArb.Simple; // how to arbitrate between points of tied value
        public BlockHandling _block = BlockHandling.Avoid; // how to treat blockages
        public float _max = -1; // the maximum path distance
        public float _divertMultOverride = -1; // multiplier to maximum divert distance that will be considered (if choosing a longer diversion around a blockage)

        public CSNavProfile(CSNavBlockRules blockRules, NavArb arb, BlockHandling block, float max, float divertMultOverride)
        {
            // TODO we're going to get errors if blockRules is null, and sometimes it is
            BlockRules = blockRules;
            _arb = arb;
            _block = block;
            _max = max;
            _divertMultOverride = divertMultOverride;
        }

        // check if the provided node is passable at all
        // if searching for a point, it will find a node at the point then check with that
        // if there is no node, it is treated as blocked
        // tryMove is for checking if a node is blocked BY trying to enter (e.g. entering the node causes something to appear suddenly, blocking the node)
        public BlockType CheckBlocked(Vector3 point, bool tryMove)
        {
            return BlockRules.PointBlocked(point);
        }
        public virtual BlockType CheckBlocked(CSNavNode node, bool tryMove)
        {
            return BlockRules.NodeBlocked(node);
        }

        // check for the actual movement cost of moving into the provided node
        public virtual float MoveCost(CSNavNode node)
        {
            return 1f;
        }

        // check for the weight this node should be given for path finding
        // HIGH weight means AVOID (e.g. to avoid moving through dangerous spaces unless necessary)
        // defaults to just the movement cost
        public virtual float MoveWeight(CSNavNode node)
        {
            return MoveCost(node);
        }
    }
}