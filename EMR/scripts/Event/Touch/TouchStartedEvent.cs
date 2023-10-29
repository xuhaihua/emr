using UnityEngine;
using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 触摸EventData类
    /// </summary>
    public class TouchStartedEventData : EventData
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
        public HandTrackingInputEventData original;
    }

    /// <summary>
    /// 触摸事件类
    /// </summary>
    [System.Serializable]
    public class TouchStartedEvent : ColliderEvent<TouchStartedEventData>
    {
        public TouchStartedEvent(Node node) : base(node)
        {
        }
    }
}
