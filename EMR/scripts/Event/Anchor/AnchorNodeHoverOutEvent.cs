using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 锚hoverOut EventData类
    /// </summary>
    public class AnchorNodeOutEventData : EventData
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
    /// 锚hoverOut事件类
    /// </summary>
    [System.Serializable]
    public class AnchorNodeOutEvent : EMREvent<AnchorNodeOutEventData>
    {
    }
}
