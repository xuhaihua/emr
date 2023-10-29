using Microsoft.MixedReality.Toolkit.UI;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 节点操纵结束EventData类
    /// </summary>
    public class ManipulationEndedEventData : EventData
    {
        /// <summary>
        /// 被操纵的节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 原始事件数据
        /// </summary>
        public ManipulationEventData original;
    }

    /// <summary>
    /// 节点操纵结束事件类
    /// </summary>
    [System.Serializable]
    public class ManipulationEndedEvent : EMREvent<ManipulationEndedEventData>
    {
    }
}
