using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发本能交互操作事件能力
    /// </summary>
    public interface IManipulationEventFeature
    {
        /// <summary>
        /// 操作开始事件
        /// </summary>
        ManipulationStartedEvent onManipulationStarted { get; }

        /// <summary>
        /// 操作结束事件
        /// </summary>
        ManipulationEndedEvent onManipulationEnded { get; }
    }
}

