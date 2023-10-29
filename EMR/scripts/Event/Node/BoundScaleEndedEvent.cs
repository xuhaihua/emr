using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 尺寸改变结束EventData类
    /// </summary>
    public class BoundScaleEndedEventData: EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;
    }

    /// <summary>
    /// 尺寸改变结束事件类
    /// </summary>
    [System.Serializable]
    public class BoundScaleEndedEvent : EMREvent<BoundScaleEndedEventData>
    {
    }
}


