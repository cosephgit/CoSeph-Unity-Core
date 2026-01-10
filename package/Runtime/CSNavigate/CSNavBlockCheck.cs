using UnityEngine;

namespace CoSeph.Core
{
    // Clear:
    // No blockage, movement is permitted now.
    // Pawn:
    // A pawn is blocking - might potentially move or be moveable,
    // but movement will not be permitted
    // Pawns can be allowed to plan paths through other pawns
    // (e.g. to stack up in a tight corridor)
    // Block:
    // An object is blocking and is not expected to ever move
    // Pawns can not move through and will never attempt to plan through a block
    public enum BlockType
    {
        Clear,
        Pawn,
        Block
    }

    [CreateAssetMenu(fileName = "NavBlockCondition", menuName = "CS/Navigation", order = 0)]
    public class CSNavBlockRules : ScriptableObject
    {
        public int layersPawn = -1;
        public int layersBlock = -1;

        /// <summary>
        /// Determines whether a world-space point is blocked.
        /// </summary>
        /// <param name="point">World-space position to evaluate.</param>
        /// Must be greater than zero for traversal to be permitted.
        /// Ignored when <see cref="BlockType.Block"/> is returned.
        /// </param>
        /// <returns>
        /// BlockType.Clear or BlockType.Pawn for passable results,
        /// or BlockType.Block if traversal is impossible.
        /// </returns>
        public virtual BlockType PointBlocked(Vector3 point)
        {
            if (CSNavigate.Navigation == NavType.Nav3D)
            {
                Collider[] blocked;
                if (layersBlock > 0)
                {
                    blocked = Physics.OverlapSphere(point, 0, layersBlock);
                    if (blocked.Length > 0)
                        return BlockType.Block;
                }
                if (layersPawn > 0)
                {
                    blocked = Physics.OverlapSphere(point, 0, layersPawn);

                    if (blocked.Length > 0)
                        return BlockType.Pawn;
                }
            }
            else // 2D free or 2D ortho
            {
                Collider2D blocked;
                if (layersBlock > 0)
                {
                    blocked = Physics2D.OverlapPoint(point, layersBlock);
                    if (blocked)
                        return BlockType.Block;
                }
                if (layersPawn > 0)
                {
                    blocked = Physics2D.OverlapPoint(point, layersBlock);

                    if (blocked)
                        return BlockType.Pawn;
                }
            }
            return BlockType.Clear;
        }
        /// <summary>
        /// Determines whether a navigation node is blocked.
        /// </summary>
        public virtual BlockType NodeBlocked(CSNavNode node)
        {
            if (node)
                return PointBlocked(node.transform.position);

            return BlockType.Block;
        }
    }
}