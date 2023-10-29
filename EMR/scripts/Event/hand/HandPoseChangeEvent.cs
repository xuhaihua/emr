using UnityEngine;
using EMR.Entity;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace EMR.Event
{
    /// <summary>
    /// hand pose change EventData类
    /// </summary>
    public class HandPoseChangeEventData : EventData
    {
        /// <summary>
        /// 当前手的位置
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// 当前手的旋转
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// 原始事件状态对象
        /// </summary>
        public SourcePoseEventData<MixedRealityPose> original;
    }

    public class HandPoseChangeEvent : EMREvent<HandPoseChangeEventData>
    {
    }
}
