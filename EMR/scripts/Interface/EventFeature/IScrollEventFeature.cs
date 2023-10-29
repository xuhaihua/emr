using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有内容滚动能力
    /// </summary>
    public interface IScrollEventFeature
    {
        ScrollEvent onScroll { get; }

        ScrollReadyEvent onScrollReady { get; }
    }
}
