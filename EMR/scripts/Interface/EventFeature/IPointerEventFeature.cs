using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发指针事件能力
    /// </summary>
    public interface IPointerEventFeature
    {
        /// <summary>
        /// 按下事件
        /// </summary>
        DownEvent onDown { get; }

        /// <summary>
        /// 释放事件
        /// </summary>
        UpEvent onUp { get; }

        /// <summary>
        /// 单击事件
        /// </summary>
        ClickEvent onClick { get;}

        /// <summary>
        /// 拖动事件
        /// </summary>
        DraggedEvent onDragged { get; }
    }
}

