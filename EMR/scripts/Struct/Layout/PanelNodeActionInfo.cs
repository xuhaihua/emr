using EMR.Entity;

namespace EMR.Struct
{
    public struct PanelNodeActionInfo
    {
        public PanelNodeAction action;

        /// <summary>
        /// 节点原始父节点
        /// </summary>
        public PanelNode originalParent;

        /// <summary>
        /// 当前操作的节点
        /// </summary>
        public PanelNode current;

        /// <summary>
        /// 节点当前结构下的父节点
        /// </summary>
        public PanelNode currentParent;
    }
}