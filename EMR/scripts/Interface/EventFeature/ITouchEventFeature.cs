using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发触摸事件能力
    /// </summary>
    public interface ITouchEventFeature
    {
        /// <summary>
        /// 触摸事件
        /// </summary>
        TouchStartedEvent onTouchStarted { get; }

        /// <summary>
        /// 触摸更新事件
        /// </summary>
        TouchUpdateEvent onTouchUpdate { get; }

        /// <summary>
        /// 触摸完成事件
        /// </summary>
        TouchCompletedEvent onTouchCompleted { get; }
    }
}

