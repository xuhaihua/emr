using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 锚hoverEntry EventData类
    /// </summary>
    public class AnchorNodeHoverEventData : EventData
    {
        /// <summary>
        /// 锚
        /// </summary>
        public Anchor anchor;

        /// <summary>
        /// 感兴趣节点
        /// </summary>
        public Node interestNode;
    }

    /// <summary>
    /// 锚hoverEntry事件类
    /// </summary>
    [System.Serializable]
    public class AnchorNodeHoverEvent : EMREvent<AnchorNodeHoverEventData>
    {
    }
}
