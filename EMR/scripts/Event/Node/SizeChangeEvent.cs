using EMR.Entity;
using EMR.Struct;

namespace EMR.Event
{
    /// <summary>
    /// 尺寸改变EventData类
    /// </summary>
    public class SizeChangeEventData: EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 原尺寸
        /// </summary>
        public SizeData oldSize;

        /// <summary>
        /// 当前尺寸
        /// </summary>
        public SizeData currentSize;

        /// <summary>
        /// 阻止事件默认行为
        /// </summary>
        public override void preventDefault()
        {
            base.preventDefault();
            this.target.revertSize();
        }
    }

    /// <summary>
    /// 尺寸改变事件类
    /// </summary>
    [System.Serializable]
    public class SizeChangeEvent : EMREvent<SizeChangeEventData>
    {
    }
}


