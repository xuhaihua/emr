using UnityEngine;
using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 触摸完成EventData类
    /// </summary>
    public class TouchCompletedEventData : EventData
    {
        /// <summary>
        /// 指针目标对象
        /// </summary>
        public Node target;

        /// <summary>
        /// 触摸点的空间坐标
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// 触摸的法线
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
    /// 触摸完成事件类
    /// </summary>
    [System.Serializable]
    public class TouchCompletedEvent : ColliderEvent<TouchCompletedEventData>
    {
        public TouchCompletedEvent(Node node) : base(node)
        {
        }
    }
}

