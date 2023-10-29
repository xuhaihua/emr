using EMR.Entity;
using EMR.Struct;

namespace EMR.Event
{
    /// <summary>
    /// 位置改变EventData类
    /// </summary>
    public class PositionChangeEventData : EventData
    {
        /// <summary>
        /// 目标节点
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
        /// 原尺寸
        /// </summary>
        public SizeData oldSize;

        /// <summary>
        /// 阻止事件默认行为
        /// </summary>
        public override void preventDefault()
        {
            base.preventDefault();
            this.target.revertPosition();
        }
    }

    /// <summary>
    /// 位置改变事件类
    /// </summary>
    [System.Serializable]
    public class PositionChangeEvent : EMREvent<PositionChangeEventData>
    {
    }
}
