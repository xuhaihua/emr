using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 尺寸改变开始EventData类
    /// </summary>
    public class BoundScaleStartedEventData: EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;
    }

    /// <summary>
    /// 尺寸改变开始事件类
    /// </summary>
    [System.Serializable]
    public class BoundScaleStartedEvent : EMREvent<BoundScaleStartedEventData>
    {
    }
}


