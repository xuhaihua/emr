using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 锚hoverOut EventData类
    /// </summary>
    public class AnchorPointerOutEventData : EventData
    {
        /// <summary>
        /// 锚
        /// </summary>
        public Anchor anchor;

        /// <summary>
        /// 感兴趣的手部指针
        /// </summary>
        public Pointer interestPointer;
    }

    /// <summary>
    /// 锚hoverOut事件类
    /// </summary>
    [System.Serializable]
    public class AnchorPointerOutEvent : EMREvent<AnchorPointerOutEventData>
    {
    }
}
