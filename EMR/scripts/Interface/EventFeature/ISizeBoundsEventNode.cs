using EMR.Event;

namespace EMR
{
    public interface ISizeBoundsEventNode
    {
        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        BoundScaleStartedEvent onBoundScaleStarted { get; }

        /// <summary>
        /// 节点尺寸改变结束事件
        /// </summary>
        BoundScaleEndedEvent onBoundScaleEnded { get; }
    }
}
