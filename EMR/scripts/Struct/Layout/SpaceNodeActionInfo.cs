using EMR.Entity;

namespace EMR.Struct
{
    public struct SpaceNodeActionInfo
    {
        public SpaceNodeAction action;

        /// <summary>
        /// 节点原始父节点
        /// </summary>
        public Node originalParent;

        /// <summary>
        /// 当前操作的节点
        /// </summary>
        public Node current;

        /// <summary>
        /// 节点当前结构下的父节点
        /// </summary>
        public Node currentParent;

        public Room room;
    }
}