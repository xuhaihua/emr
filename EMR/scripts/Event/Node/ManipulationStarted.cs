using Microsoft.MixedReality.Toolkit.UI;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 节点操纵开始EventData类
    /// </summary>
    public class ManipulationStartedEventData : EventData
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
    /// 节点操纵开始事件类
    /// </summary>
    [System.Serializable]
    public class ManipulationStartedEvent : EMREvent<ManipulationStartedEventData>
    {
    }
}
