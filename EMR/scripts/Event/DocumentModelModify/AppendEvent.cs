using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素添加前EventData类
    /// </summary>
    public class AppendEventData : EventData
    {
        /// <summary>
        /// 添加的Element
        /// </summary>
        public Element target;
    }

    /// <summary>
    /// 元素添加前事件类
    /// </summary>
    [System.Serializable]
    public class AppendEvent : EMREvent<AppendEventData>
    {
    }
}
