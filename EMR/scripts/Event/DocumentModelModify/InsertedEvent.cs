using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素插入前EventData类
    /// </summary>
    public class InsertedEventData : EventData
    {
        /// <summary>
        /// 添加的Element
        /// </summary>
        public Element target;

        /// <summary>
        /// 元素是否添加成功
        /// </summary>
        public bool isSuccess = false;
    }

    /// <summary>
    /// 元素插入前事件类
    /// </summary>
    [System.Serializable]
    public class InsertedEvent : EMREvent<InsertedEventData>
    {
    }
}
