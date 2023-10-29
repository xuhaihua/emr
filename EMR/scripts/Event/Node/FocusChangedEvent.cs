using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 焦点改变EventData类
    /// </summary>
    public class FocusChangedEventData: EventData
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
    /// 焦点改变事件类
    /// </summary>
    [System.Serializable]
    public class FocusChangedEvent : ColliderEvent<FocusChangedEventData>
    {
        public FocusChangedEvent(Node node) : base(node)
        {
        }
    }
}


