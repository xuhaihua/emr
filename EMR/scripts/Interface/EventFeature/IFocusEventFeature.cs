using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发焦点事件能力
    /// </summary>
    public interface IFocusEventFeature
    {
        /// <summary>
        /// 焦点进入事件
        /// </summary>
        FocusEnterEvent onFocusEnter { get; }

        /// <summary>
        /// 焦点改变事件
        /// </summary>
        FocusChangedEvent onFocusChanged { get; }

        /// <summary>
        /// 焦点退出事件
        /// </summary>
        FocusExitEvent onFocusExit { get; }
    }
}