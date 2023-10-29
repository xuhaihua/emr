using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 锚hover EventData类
    /// </summary>
    public class AnchorPointerHoverEventData : EventData
    {
        /// <summary>
        /// 锚
        /// </summary>
        public Anchor anchor;

        /// <summary>
        /// 感兴趣手部指针
        /// </summary>
        public Pointer interestPointer;
    }

    /// <summary>
    /// 锚hover事件类
    /// </summary>
    [System.Serializable]
    public class AnchorPointerHoverEvent : EMREvent<AnchorPointerHoverEventData>
    {
    }
}
