using EMR;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 元素销毁EventData类
    /// </summary>
    public class DestoryEventData : EventData
    {
        /// <summary>
        /// 销毁的Element
        /// </summary>
        public Element target;
    }

    /// <summary>
    /// 元素销毁事件类
    /// </summary>
    [System.Serializable]
    public class DestoryEvent : EMREvent<DestoryEventData>
    {
    }
}
