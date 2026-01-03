using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavNode
// created 17/10/23
// last modified 16/3/24
// version 0.1

// stores permissible pawn positions for creating pathfinding data
// might do other things later e.g. mark good places for doors/loot/etc
// depends on CSNavigate singleton

namespace CoSeph.Core
{
    public enum NodeStatus
    {
        Clean, // has not been set up for the current path find yet
        Initial, // has been initialised
        Calculated, // path finding has been calculated to here
        Start // this is the origin
    }

    public class CSNavNode : MonoBehaviour
    {
        // nodes to navigate to adjacent nodes
        public List<CSNavNode> nodeConnections { get; private set; }
        public CSNavNode pathPrev { get; private set; }

        // pathfinding data
        private NodeStatus status;
        // used for A*
        private float pathFCost = 0; // total
        private float pathGCost = 0; // path so far
        private float pathHCost = 0; // estimated path left REMEMBER THIS MUST BE THE SHORTEST POSSIBLE DISTANCE EVER
        public bool nodePassable { get; private set; } = true; // if false this node is a destination only and can't be pathed through
        public float pathDistance = 0; // used for Djikstra

        public float nodeValue; // general-purpose node evaluation variable can be used for anything e.g. sorting nodes or prioritising routes

        // this is called after all CSNavNodes are placed to calculate node adjacency
        // index is just used to name the node for easier identification
        public void Initialise(string key, int index, bool nodePassableSet = true)
        {
            name = key + index;
            nodePassable = nodePassableSet;
            nodeConnections = new List<CSNavNode>();

            PathClear();
        }

        // maxConnection is the maximum radius for any connected nodes
        public void BuildConnections(float maxConnectDist)
        {
            nodeConnections.Clear();

            switch (CSNavigate.instance.type)
            {
                default:
                case NavType.Nav2DFree:
                case NavType.Nav2DOrtho: // should we validate nodes for ortho movement or just assume they'll only be in connect dist if they're valid?
                    {
                        Collider2D[] nodesAdjacent = Physics2D.OverlapCircleAll(transform.position, maxConnectDist, CSNavigate.instance.LayerNav());

                        foreach (Collider2D node in nodesAdjacent)
                        {
                            CSNavNode navNode = node.GetComponent<CSNavNode>();
                            if (navNode && navNode != this)
                                nodeConnections.Add(navNode);
                        }
                        break;
                    }
                case NavType.Nav3D:
                    {
                        Collider[] nodesAdjacent = Physics.OverlapSphere(transform.position, maxConnectDist, CSNavigate.instance.LayerNav());

                        foreach (Collider node in nodesAdjacent)
                        {
                            CSNavNode navNode = node.GetComponent<CSNavNode>();
                            if (navNode && navNode != this)
                                nodeConnections.Add(navNode);
                        }
                        break;
                    }
            }
        }

        // initiate pathfinding from this square to the target square
        // pawnvalue is the cost multiplier for a space containing an obstacle which might move (e.g. a friendly pawn) - 0 means do not path through them
        public void PathFind(CSNavNode target, CSNavNode previous, float pawnValue, float distMax, CSNavProfile profile)
        {
            CSNavNode optimalNode = null;

            // initialise all uninitialised adjacent nodes
            for (int i = 0; i < nodeConnections.Count; i++)
            {
                nodeConnections[i].PathNodeSet(target, this, pathGCost, pawnValue, profile);
            }

            if (previous == null)
            {// this is the first node in the search
                status = NodeStatus.Start;
            }
            else
            {
                status = NodeStatus.Calculated;
            }

            // add this node to the dirty node list so it can be cleaned later
            CSNavigate.instance.navNodeDirty.Add(this);

            float optimalDist = Mathf.Infinity;
            float optimalHDist = Mathf.Infinity;
            foreach (CSNavNode node in CSNavigate.instance.navNodeMap)
            {
                if (node.status == NodeStatus.Initial)
                {
                    if (node.pathFCost < optimalDist)
                    {
                        optimalNode = node;
                        optimalDist = node.pathFCost;
                        optimalHDist = node.pathHCost;
                    }
                    else if (node.pathFCost == optimalDist)
                    {
                        bool update = false;

                        if (node.pathHCost < optimalHDist)
                            update = true;
                        else if (node.pathHCost == optimalHDist)
                        {
                            switch (profile.arb)
                            {
                                default:
                                case NavArb.Simple:
                                    update = false;
                                    break;
                                case NavArb.Random:
                                    update = CSUtils.RandomBool();
                                    break;
                                case NavArb.Direct:
                                    Vector3 offset = target.transform.position - node.transform.position;
                                    Vector3 offsetCurrent = target.transform.position - optimalNode.transform.position;

                                    if (Mathf.Abs(offsetCurrent.x) > Mathf.Abs(offsetCurrent.y))
                                    {
                                        if (CSNavigate.instance.type == NavType.Nav3D)
                                        {
                                            if (Mathf.Abs(offsetCurrent.x) > Mathf.Abs(offsetCurrent.z))
                                                update = (Mathf.Abs(offset.x) < Mathf.Abs(offsetCurrent.x));
                                            else
                                                update = (Mathf.Abs(offset.z) < Mathf.Abs(offsetCurrent.z));
                                        }
                                        else
                                            update = (Mathf.Abs(offset.x) < Mathf.Abs(offsetCurrent.x));
                                    }
                                    else
                                    {
                                        if (CSNavigate.instance.type == NavType.Nav3D)
                                        {
                                            if (Mathf.Abs(offsetCurrent.y) > Mathf.Abs(offsetCurrent.z))
                                                update = (Mathf.Abs(offset.y) < Mathf.Abs(offsetCurrent.y));
                                            else
                                                update = (Mathf.Abs(offset.z) < Mathf.Abs(offsetCurrent.z));
                                        }
                                        else
                                            update = (Mathf.Abs(offset.y) < Mathf.Abs(offsetCurrent.y));
                                    }
                                    break;
                            }
                        }

                        if (update)
                        {
                            optimalNode = node;
                            optimalDist = node.pathFCost;
                            optimalHDist = node.pathHCost;
                        }
                    }
                }
            }

            if (optimalNode)
            {
                // have found a node to take next
                if (optimalNode == target)
                {
                    // target found, pathfinding complete
                    // the pathing has now been created and is stored in the nodes, so the path does not need to be identified here
                }
                else
                {
                    optimalNode.PathFind(target, this, pawnValue, distMax, profile);
                }
            }
        }

        // set up the pathfinding data to this node
        // set up f value for this node
        public void PathNodeSet(CSNavNode target, CSNavNode previous, float currentG, float pawnValue, CSNavProfile profile)
        {
            BlockType obstacle = BlockType.Clear;
            float difficulty = 1f;
            // only set it up if it's not been set up already
            if (status == NodeStatus.Clean)
            {
                // add to dirty node list
                CSNavigate.instance.navNodeDirty.Add(this);

                // only need to check for obstacles when this node isn't the destination
                if (target != this)
                {
                    if (!nodePassable)
                    {
                        status = NodeStatus.Calculated;
                        return;
                    }

                    //obstacle = CSNavigate.instance.blockCheck.NodeBlocked(this, out difficulty);

                    obstacle = profile.CheckBlocked(this, false);

                    if ((obstacle == BlockType.Pawn && pawnValue <= 0)
                        || obstacle == BlockType.Block)
                    {
                        status = NodeStatus.Calculated; // blocked, don't consider this node for pathfinding
                        return;
                    }
                    else
                    {
                        difficulty = profile.MoveCost(target);
                    }
                }

                // G cost is modified by this node difficulty - usable for things like difficult terrain
                if (obstacle == BlockType.Pawn) // pawnvalue is necessarily > 0 to get here
                {
                    // so allow pathing through pawns, but adjust the space value
                    pathGCost = currentG + NodeDistance(previous) * pawnValue * difficulty;
                    //pathHCost = NodeDistance(target) * pawnValue;
                    pathHCost = NodeDistance(target);
                }
                else
                {
                    pathGCost = currentG + NodeDistance(previous) * difficulty;
                    pathHCost = NodeDistance(target);
                }
                pathFCost = pathGCost + pathHCost;
                if (profile.max > 0 && pathFCost > profile.max)
                {
                    status = NodeStatus.Calculated; // too far, don't consider this node for pathfinding
                    return;
                }
                status = NodeStatus.Initial; // this node has been initialised
                pathPrev = previous;
            }
        }

        // clears all pathfinding data from this node
        // this needs to be done before all pathfinding checks
        public void PathClear()
        {
            status = NodeStatus.Clean;
            pathGCost = 0;
            pathHCost = 0;
            pathFCost = 0;
            pathDistance = 0;
            pathPrev = null;
            nodeValue = 0f;
        }

        // calculates the distance from this node to the target point (includes using for H distance)
        // calculates with the navigation type in CSNavigate
        public float NodeDistance(CSNavNode target)
        {
            return NodeDistance(target.transform.position);
        }
        public float NodeDistance(Vector3 target)
        {
            switch (CSNavigate.instance.type)
            {
                default:
                case NavType.Nav3D:
                    return (transform.position - target).magnitude;
                case NavType.Nav2DOrtho:
                    return CSUtils.OrthogonalDist(transform.position, target);
                case NavType.Nav2DFree:
                    return ((Vector2)(transform.position - target)).magnitude;
            }
        }

        // currently just returns this node if it's a valid connection, else null
        // TODO probably rework this to do more, or at least rename it
        public CSNavNode PathDirFrom(CSNavNode origin)
        {
            if (origin.nodeConnections.Contains(this)) return this;

            return null;
        }
    }
}