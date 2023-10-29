using EMR.Entity;
using EMR.Struct;

namespace EMR.Event
{
    /// <summary>
    /// 滚动EventData类
    /// </summary>
    public class ScrollEventData : EventData
    {
        /// <summary>
        /// 目标对象
        /// </summary>
        public Node target;

        /// <summary>
        /// 原位置
        /// </summary>
        public PositionData oldPosition;

        /// <summary>
        /// 当前位置
        /// </summary>
        public PositionData currentPosition;

        /// <summary>
        /// 阻止事件默认行为
        /// </summary>
        public override void preventDefault()
        {
            base.preventDefault();
        }
    }

    /// <summary>
    /// 滚动事件类
    /// </summary>
    [System.Serializable]
    public class ScrollEvent : EMREvent<ScrollEventData>
    {
    }
}
