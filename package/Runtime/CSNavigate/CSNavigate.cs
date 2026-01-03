using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSNavigate
// created 17/10/23
// last modified 6/3/24

/*
 * Adapting this to a general-purpose pathfinder - what does it need?
 * Pathfinding in 2 dimenions, 3 dimensions, on a hex grid
 * so what are the possible options?
 * the general purpose option is to require all nodes to have a collider so they can be found by location easily
 * this is more expensive and messy than other options though
 * for example nodes could be identified in a dictionary by location vector
 * would be cleaner but not as resilient or as flexible
 * screw it take the dirty route, anything that needs to be faster can be done another way
 */


/*
 * got a REALLY FREAKING WEIRD bug
 * sometimes pathfinding results in an angular path
 * how can this possibly happen???
 * 
 * XXXXXXX
 * ZZZZZZZ
 * XXXXXXX
 * 
 * somehow this path (diverting) is being seen as shorter, how can this possibly happpen?
 * XXXXXXX
 * ZZZZZZZ
 * XXXXZZX
 * so I could do a really ugly hack to detect it...
 * just every time a route is checked, check if there's an adjacent point in the path which ISNT the next point in the path
 * then work backwards from there
 * need full logging of all enemy moves so when it happens I can see the process
 * as it's hard to recreate, very unpredictable (seems MOST common with straight routes, at least)
 * 
 */

namespace CoSeph.Core
{
    public enum NavType
    {
        Nav2DOrtho,
        Nav2DFree,
        Nav3D
        // add hexagonal later
    }

    public enum BlockHandling
    {
        Divert, // will path around obstacles if the unblocked path is no more than PATHDIVERTMULT times as long
        Ignore, // will ignore all obstacles for pathing and just find the shortest route even if blocked
        Avoid // will avoid all obstacles and never return a blocked path
    }

    // nav arbitration - used when a node can be reached with multiple paths of the same path length
    public enum NavArb
    {
        Simple, // simply keep the first node found with the indicated length
        Random, // randomised at each conflict
        Direct // select the node with the shortest direct distance to the target (defaults to Simple on a tie, to be deterministic)
    }


    public enum NodeBroken
    {
        Ignore, // broken nodes should be reported but otherwise ignored
        Destroy // broken nodes should be destroyed
    }

    public class CSNavigate : MonoBehaviour
    {
        public static CSNavigate instance;
        [field: Header("Pathfinding settings")]
        [field: SerializeField] public NavType type { get; private set; } = NavType.Nav2DOrtho;
        //[field: SerializeField] public CSNavBlockCheck blockCheck { get; private set; } // interface for checking for blocked nodes
        [SerializeField] private float pathDivertPawnWeight = 4f;
        [SerializeField] private float pathDivertMult = 4; // when accepting blocked routes, an unblocked route will be picked if it is no more than this times the blocked route
        [SerializeField] private float pathDivertFlat = 4; // when accepting blocked routes, an unblocked route will be picked if it is no more than this amount more than the blocked route
        [SerializeField] private float pathFindMax = 20f;
        [Header("Layer names")]
        [SerializeField] private string layerNavString = "NavNode";
        [SerializeField] private string layerNavStringObstacle = "NavNodeEdge"; // obstacles that may appear between nodes as a barrier e.g. windows, doors
        [HideInInspector] public List<CSNavNode> navNodeMap = new List<CSNavNode>();
        [HideInInspector] public List<CSNavNode> navNodeDirty = new List<CSNavNode>();

        private LayerMask layerNav;
        private LayerMask layerPawn;
        private LayerMask layerNavEdge;

        private void Awake()
        {
            if (instance)
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else instance = this;
        }

        public void ClearNavNetwork()
        {
            for (int i = 0; i < navNodeMap.Count; i++)
                Destroy(navNodeMap[i].gameObject);
            for (int i = 0; i < navNodeDirty.Count; i++)
                Destroy(navNodeDirty[i].gameObject);
            navNodeMap.Clear();
            navNodeDirty.Clear();
        }

        public LayerMask LayerNav()
        {
            if (layerNav == 0) layerNav = LayerMask.GetMask(layerNavString);
            return layerNav;
        }

        public LayerMask LayerNavEdge()
        {
            if (layerNavEdge == 0) layerNavEdge = LayerMask.GetMask(layerNavStringObstacle);
            return layerNavEdge;
        }

        public void Initialise()
        {
            navNodeDirty.Clear();

            for (int i = 0; i < navNodeMap.Count; i++)
            {
                Destroy(navNodeMap[i].gameObject);
            }
            navNodeMap.Clear();
        }

        void CleanNavNodes()
        {
            for (int i = 0; i < navNodeDirty.Count; i++)
            {
                navNodeDirty[i].PathClear();
            }
            navNodeDirty.Clear();
        }

        // gets the node at the provided point
        // if closest == true, it finds the nearest node if there isn't one at the point
        public CSNavNode GetNode(Vector3 pos, bool closest = false)
        {
            CSNavNode originNode = null;

            switch (type)
            {
                default:
                case NavType.Nav2DOrtho:
                case NavType.Nav2DFree:
                    {
                        Collider2D originCollider = Physics2D.OverlapPoint(pos, LayerNav());
                        if (originCollider)
                            originNode = originCollider.GetComponent<CSNavNode>();
                        break;
                    }
                case NavType.Nav3D:
                    {
                        // BIG NOTE FOR LATER
                        // IF DOING 3D NAVIGATION AND NOTHING WORKS, CHECK IF OVERLAPSPHERE WORKS WITH RADIUS 0
                        Collider[] originCollider = Physics.OverlapSphere(pos, 0, LayerNav());
                        if (originCollider.Length > 0)
                            originNode = originCollider[0].GetComponent<CSNavNode>();
                        break;
                    }
            }

            if (!originNode && closest)
            {
                // have not found a node nearby and willing to take the closest node, so cycle through all nodes to find the closest
                float closestDist = Mathf.Infinity;

                for (int i = 0; i < navNodeMap.Count; i++)
                {
                    float dist = (pos - navNodeMap[i].transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        originNode = navNodeMap[i];
                    }
                }
            }

            return originNode;
        }

        // checks if the path between two nodes is blocked by a CSNavNodeEdge object
        public bool PathBlocked(CSNavNode origin, CSNavNode target)
        {
            return PathBlocked(origin.transform.position, target.transform.position);
        }
        public bool PathBlocked(Vector3 origin, Vector3 target)
        {
            Vector3 offset = target - origin;

            switch (type)
            {
                default:
                case NavType.Nav2DOrtho:
                    {
                        // in ortho just check the mid point between nodes
                        Vector2 check = origin + 0.5f * offset;
                        Collider2D collide = Physics2D.OverlapPoint(check, LayerNavEdge());

                        if (collide)
                        {
                            CSNavNodeEdge obstacle = collide.GetComponent<CSNavNodeEdge>();

                            if (obstacle && obstacle.blocking) return true;
                        }
                        break;
                    }
                case NavType.Nav2DFree:
                    {
                        RaycastHit2D hit = Physics2D.Raycast(origin, offset, offset.magnitude, LayerNavEdge());
                        if (hit)
                        {
                            CSNavNodeEdge obstacle = hit.transform.GetComponent<CSNavNodeEdge>();

                            if (obstacle && obstacle.blocking) return true;
                        }
                        break;
                    }
                case NavType.Nav3D:
                    {
                        RaycastHit[] hit = Physics.RaycastAll(origin, offset, offset.magnitude, LayerNavEdge());
                        if (hit.Length > 0)
                        {
                            CSNavNodeEdge obstacle = hit[0].transform.GetComponent<CSNavNodeEdge>();

                            if (obstacle && obstacle.blocking) return true;
                        }
                        break;
                    }
            }

            return false;
        }

        // pathfinds from origin to target
        // range is the total travel distance to the target
        // returns the result as a Vector3 list of moves to reach the target IN REVERSE ORDER
        // an empty list means no path could be found OR the path is too long

        // distmax negative means use default limit, distmax zero means no limit, distmax positive means assign limit to value

        public List<CSNavNode> Pathfind(Vector3 origin, Vector3 target, CSNavProfile profile, out bool passed)
        {
            CSNavNode originNode = GetNode(origin);
            CSNavNode targetNode = GetNode(target);

            return Pathfind(originNode, targetNode, profile, out passed);
        }
        public List<CSNavNode> Pathfind(Vector3 origin, CSNavNode targetNode, CSNavProfile profile, out bool passed)
        {
            CSNavNode originNode = GetNode(origin);

            return Pathfind(originNode, targetNode, profile, out passed);
        }
        public List<CSNavNode> Pathfind(CSNavNode originNode, Vector3 target, CSNavProfile profile, out bool passed)
        {
            CSNavNode targetNode = GetNode(target);

            return Pathfind(originNode, targetNode, profile, out passed);
        }
        public List<CSNavNode> Pathfind(CSNavNode originNode, CSNavNode targetNode, CSNavProfile profile, out bool passed)
        {
            List<CSNavNode> result = new List<CSNavNode>();
            float divertMult = pathDivertMult;
            float distMax = profile.max;
            if (profile.divertMultOverride > 0)
                divertMult *= profile.divertMultOverride;

            if (distMax < 0) distMax = pathFindMax;

            if (!originNode || !targetNode)
            {
                // either the origin or target is not on a node position
                passed = false;
                return result;
            }
            if (originNode == targetNode)
            {
                // yes you can reach your current node from your current node, though you might need to ask why this question was necessary
                passed = true;
                return result;
            }

            if (distMax > 0)
            {
                float fullHDist;

                if (type == NavType.Nav2DOrtho)
                    fullHDist = CSUtils.OrthogonalDist(originNode.transform.position, targetNode.transform.position);
                else
                    fullHDist = (targetNode.transform.position - originNode.transform.position).magnitude;

                if (fullHDist > distMax) // the shortest possible path is too long
                {
                    passed = false;
                    return result;
                }
            }

            List<CSNavNode> resultDirect = new List<CSNavNode>(); // this is used to make a direct path, ignoring pawn obstacles, so enemies stack up if they can't find an open route or if the open route is too long
            List<CSNavNode> testList = new List<CSNavNode>();

            CleanNavNodes();
            // build the pathfinding data to the target
            if (profile.block == BlockHandling.Avoid || profile.block == BlockHandling.Divert)
                originNode.PathFind(targetNode, null, 0f, distMax, profile);
            else // acceptBlocked == BlockHandling.Ignore, so just go straight in with the direct path search
                originNode.PathFind(targetNode, null, 1f, distMax, profile);

            testList = BuildPath(targetNode);
            if (testList.Count > 0) result.AddRange(testList);

            if (profile.block == BlockHandling.Divert)
            {
                // for the divert behaviour we need to pathfind again looking for the most favourable direct path
                CleanNavNodes();
                // direct path search and set nodes with pawns in more expensive (to choose the blocked path with the least blocking pawns)
                // TODO this needs to be refined, to detect the path length before it reaches a pawn and choose the route that allows the most movement
                originNode.PathFind(targetNode, null, pathDivertPawnWeight, distMax * pathDivertPawnWeight, profile);
                testList = BuildPath(targetNode);
                if (testList.Count > 0) resultDirect.AddRange(testList);
            }

            // we have a route to the target created now, it is in reverse order from LAST move to FIRST move
            if ((distMax > 0 && result.Count > distMax) || result.Count == 0
                || (profile.block == BlockHandling.Divert && (resultDirect.Count * divertMult < result.Count || resultDirect.Count + pathDivertFlat < result.Count)))
            {
                // either the open route is too long, there is no open route, or the open route is over PATHDIVERTMULTx the length of the direct route
                // or the open route is over pathDivertFlat + the length of the direct route
                // this will allow for enemies that try to wrap around the player in melee, but wont go on a massive diversion to find an open route

                result.Clear();

                if (profile.block == BlockHandling.Divert)
                {
                    // then try to accept a path that is not clear
                    if (resultDirect.Count > 0 && (resultDirect.Count <= distMax || distMax <= 0))
                    {
                        result = resultDirect;
                    }
                }
            }

            if (result.Count > 0) passed = true;
            else passed = false;

            return result;
            //return Pathfind(originNode, targetNode, out passed, profile.block, profile.arb, profile.max, profile.divertMultOverride);
        }

        // BuildPath
        // call after pathfinding data has been generated to built the list of the path nodes to the target
        // the START of the returned list will be the LAST move required to reach the target
        // each entry in the list the node for the direction required for that move
        private List<CSNavNode> BuildPath(CSNavNode target)
        {
            List<CSNavNode> path = new List<CSNavNode>();
            List<CSNavNode> pathSegment = new List<CSNavNode>();
            CSNavNode testPath;

            // don't need to add the target square to the list - we just want to get adjacent! 
            // TODO later on we WILL want to get into the target for objectives/switches/etc but not needed right now
            if (target.pathPrev)
            {
                testPath = target.PathDirFrom(target.pathPrev);

                if (target == target.pathPrev)
                {
                    Debug.LogError("BuildPath target == target.pathPrev");
                    return path;
                }

                if (testPath == null)
                {
                    Debug.LogError("BuildPath failed with invalid testpath");
                    return null;
                }
                path.Add(testPath);
                pathSegment = BuildPath(target.pathPrev);
                if (pathSegment == null)
                {
                }
                else
                {
                    path.AddRange(pathSegment);
                }
                return path;
            }
            else
            {
                return path;
            }
        }

        // returns a list of all nodes which can be reached within the provided movement distance
        // you can use the pathDistance in the returned nodes to check the distance, but only immediately after as it may be cleared at any later moment
        // use negative distMax to set no maximum pathing distance
        public List<Vector3> PathfindAllVectors(Vector3 pos, CSNavProfile profile)
        {
            List<Vector3> results = new List<Vector3>();
            List<CSNavNode> resultsNode = PathfindAll(pos, profile);

            for (int i = 0; i < resultsNode.Count; i++)
                results.Add(resultsNode[i].transform.position);

            return results;
        }
        public List<CSNavNode> PathfindAll(Vector3 pos, CSNavProfile profile)
        {
            CSNavNode nodeOrigin = GetNode(pos);

            if (nodeOrigin)
                return PathfindAll(nodeOrigin, profile);

            Debug.LogWarning("CSNavigate.PathfindAll called with pos " + pos + " and couldn't find a nav node");

            return new List<CSNavNode>();
        }
        public List<CSNavNode> PathfindAll(CSNavNode originNode, CSNavProfile profile)
        {
            List<CSNavNode> targets = new List<CSNavNode>();
            Queue<CSNavNode> frontier = new Queue<CSNavNode>();
            int crapout = 10000;
            float distMax = profile.max;

            if (distMax < 0) distMax = pathFindMax;

            CleanNavNodes();

            frontier.Enqueue(originNode);
            targets.Add(originNode);
            navNodeDirty.Add(originNode);

            while (frontier.Count > 0 && crapout > 0)
            {
                CSNavNode currentNode = frontier.Dequeue();

                if (distMax > 0 && currentNode.pathDistance >= distMax)
                    continue;

                List<CSNavNode> neighbors = currentNode.nodeConnections;
                foreach (CSNavNode neighbor in neighbors)
                {
                    float distance;
                    float difficulty = 1f;
                    BlockType nodeBlocked = profile.CheckBlocked(neighbor, false);//  blockCheck.NodeBlocked(neighbor, out difficulty);

                    if (neighbor != originNode)
                    {
                        if (nodeBlocked == BlockType.Block
                            || (nodeBlocked == BlockType.Pawn && profile.block != BlockHandling.Ignore))
                            continue;
                    }

                    //nodeBlocked = BlockType.Clear;

                    // possibly account for treating pawns as temporary blockages here
                    //if (nodeBlocked != BlockType.Clear) continue;

                    navNodeDirty.Add(neighbor);

                    switch (type)
                    {
                        case NavType.Nav2DOrtho:
                            {
                                distance = difficulty + currentNode.pathDistance;
                                break;
                            }
                        default:
                        case NavType.Nav2DFree:
                        case NavType.Nav3D:
                            {
                                distance = (neighbor.NodeDistance(currentNode) * difficulty) + currentNode.pathDistance;
                                break;
                            }
                    }

                    if (targets.Contains(neighbor))
                    {
                        if (distance < neighbor.pathDistance)
                        {
                            // found a shorter path so re-que it to be checked again
                            neighbor.pathDistance = distance;
                            if (neighbor.nodePassable)
                                frontier.Enqueue(neighbor);
                        }
                    }
                    else
                    {
                        neighbor.pathDistance = distance;
                        if (neighbor.nodePassable)
                            frontier.Enqueue(neighbor);
                        targets.Add(neighbor);
                        crapout = 10000;
                    }
                }
                crapout--;
            }

            //Debug.Log("crapout is " + crapout);

            return targets;
        }

        public void AddNodes(List<CSNavNode> nodesNew)
        {
            navNodeMap.AddRange(nodesNew);
        }

        public void BuildNodeConnections(float maxConnectDist)
        {
            for (int i = 0; i < navNodeMap.Count; i++)
            {
                navNodeMap[i].BuildConnections(maxConnectDist);
            }
        }

        // check all node connections are valid and possibly remove any widowed nodes
        // requires a nav node as a test node, which is treated as the master and all non-connected nodes are considered widowed off from it
        // if layersAvoid is set, any node overlapping with those layers will count as blocked
        public List<Vector3> FindWidowedNodesDestroy(CSNavNode nodeTest, LayerMask layersAvoid)
        {
            List<Vector3> pointsBroken = new List<Vector3>();
            List<CSNavNode> nodesBroken = FindWidowedNodes(nodeTest, layersAvoid);

            for (int i = 0; i < nodesBroken.Count; i++)
            {
                pointsBroken.Add(nodesBroken[i].transform.position);
                DestroyNode(nodesBroken[i]);
            }

            //if (pointsBroken.Count > 0)
            //Debug.Log("test pos " + nodeTest.transform.position + " resulted in " + pointsBroken.Count + " nodes removed - " + navNodeMap.Count + " nodes left");

            return pointsBroken;
        }
        // find any widowed nodes - note this will not destroy them, just return the list of the widowed ones
        public List<CSNavNode> FindWidowedNodes(CSNavNode nodeTest, LayerMask layersAvoid)
        {
            List<CSNavNode> nodesBroken = new List<CSNavNode>();
            CSNavProfile profile = new CSNavProfile(NavArb.Direct, BlockHandling.Ignore, 0f, -1f, layersAvoid); // dummy profile just for a simple node connectivity test
            List<CSNavNode> nodeMapTest = PathfindAll(nodeTest, profile);
            //List<CSNavNode> nodeMapTest = PathfindAll(nodeTest, true, 0);

            for (int i = 0; i < navNodeMap.Count; i++)
            {
                if (!nodeMapTest.Contains(navNodeMap[i]))
                {
                    //Debug.Log("node " + navNodeMap[i] + " was not found in pathfinding test nodemap! widowed node found! " + navNodeMap[i].transform.position);
                    nodesBroken.Add(navNodeMap[i]);
                }
            }

            //if (nodesBroken.Count > 0)
            //Debug.Log("test pos " + nodeTest.transform.position + " resulted in " + nodesBroken.Count + " windowed nodes out of " + navNodeMap.Count + " nodes total");

            return nodesBroken;
        }

        // destroy the indicated node
        public void DestroyNode(CSNavNode nodeDestroy)
        {
            navNodeMap.Remove(nodeDestroy);
            Destroy(nodeDestroy.gameObject);
        }
    }
}