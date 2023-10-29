using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素插入完成EventData类
    /// </summary>
    public class DestoryedEventData : EventData
    {
        /// <summary>
        /// 目标Element
        /// </summary>
        public Element target;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool isSuccess = false;
    }

    /// <summary>
    /// 元素插入完成事件类
    /// </summary>
    [System.Serializable]
    public class DestoryedEvent : EMREvent<DestoryedEventData>
    {
    }
}

