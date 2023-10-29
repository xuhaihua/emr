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
    public class GestureSelectEventData : EventData
    {
        /// <summary>
        /// 指针目标对象
        /// </summary>
        public Node target;

        /// <summary>
        /// 击种点
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// 击种点的法线
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// 当前指针
        /// </summary>
        public object pointer;

        /// <summary>
        /// 原始事件数据
        /// </summary>
        public MixedRealityPointerEventData original;
    }

    public class GestureSelectEvent : EMREvent<HandPoseChangeEventData>
    {
    }
}
