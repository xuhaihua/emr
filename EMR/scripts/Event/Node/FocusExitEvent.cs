using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 焦点离开EventData类
    /// </summary>
    public class FocusExitEventData: EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 旧节点
        /// </summary>
        public Node oldNode;

        /// <summary>
        /// 原始事件数据
        /// </summary>
        public FocusEventData original;
    }

    /// <summary>
    /// 焦点离开事件类
    /// </summary>
    [System.Serializable]
    public class FocusExitEvent : ColliderEvent<FocusExitEventData>
    {
        public FocusExitEvent(Node node) : base(node)
        {
        }
    }
}


