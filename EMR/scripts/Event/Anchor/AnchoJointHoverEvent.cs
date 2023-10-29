using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 锚JointHover EventData类
    /// </summary>
    public class AnchorJointHoverEventData : EventData
    {
        /// <summary>
        /// 锚
        /// </summary>
        public Anchor anchor;

        /// <summary>
        /// 感兴趣关节
        /// </summary>
        public TrackedHandJoint interestJoint;

        /// <summary>
        /// 惯用手
        /// </summary>
        public Handedness handedness;
    }

    /// <summary>
    /// 锚JointHover事件类
    /// </summary>
    [System.Serializable]
    public class AnchorJointHoverEvent : EMREvent<AnchorJointHoverEventData>
    {
    }
}
