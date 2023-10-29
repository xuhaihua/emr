using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素添加完成EventData类
    /// </summary>
    public class AppendedEventData : EventData
    {
        /// <summary>
        /// 添加的Element
        /// </summary>
        public Element target;

        /// <summary>
        /// 节点是否添加成功
        /// </summary>
        public bool isSuccess = false; 
    }

    /// <summary>
    /// 元素添加完成事件类
    /// </summary>
    [System.Serializable]
    public class AppendedEvent : EMREvent<AppendedEventData>
    {
    }
}
