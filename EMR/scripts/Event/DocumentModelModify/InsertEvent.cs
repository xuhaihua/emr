using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素插入前EventData类
    /// </summary>
    public class InsertEventData : EventData
    {
        /// <summary>
        /// 添加的Element
        /// </summary>
        public Element target;
    }

    /// <summary>
    /// 元素插入前事件类
    /// </summary>
    [System.Serializable]
    public class InsertEvent : EMREvent<InsertEventData>
    {
    }
}
