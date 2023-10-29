using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 滚动就绪EventData类
    /// </summary>
    public class ScrollReadyEventData : EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;
    }

    /// <summary>
    /// 滚动就绪事件类
    /// </summary>
    [System.Serializable]
    public class ScrollReadyEvent : EMREvent<ScrollReadyEventData>
    {
    }
}
