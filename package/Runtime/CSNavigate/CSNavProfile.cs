using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavProfile
// part of the CSNavigate bundle
// this class is used to handle tailored navigation parameters e.g. custom checks for obstacles, path prioritising and move costs
// created 23/2/24
// modified 23/2/24

public class CSNavProfile
{
    public NavArb arb = NavArb.Simple; // how to arbitrate between points of tied value
    public BlockHandling block = BlockHandling.Avoid; // how to treat blockages
    public float max = -1; // the maximum path distance
    public float divertMultOverride = -1; // multiplier to maximum divert distance that will be considered (if choosing a longer diversion around a blockage)
    private int layersAvoid = -1;

    public CSNavProfile(NavArb arb, BlockHandling block, float max, float divertMultOverride, int layersAvoid = -1)
    {
        this.arb = arb;
        this.block = block;
        this.max = max;
        this.divertMultOverride = divertMultOverride;
        this.layersAvoid = layersAvoid;
    }

    // check if the provided node is passable at all
    public BlockType CheckBlocked(Vector3 point, bool tryMove)
    {
        CSNavNode pointNode = CSNavigate.instance.GetNode(point);
        if (pointNode)
            return CheckBlocked(CSNavigate.instance.GetNode(point), tryMove);
        return BlockType.Block;
    }
    public virtual BlockType CheckBlocked(CSNavNode node, bool tryMove)
    {
        if (layersAvoid > 0)
        {
            Collider2D blocked = Physics2D.OverlapPoint(node.transform.position, layersAvoid);
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
