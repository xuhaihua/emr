using UnityEngine;
using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 触摸更新ventData类
    /// </summary>
    public class TouchUpdateEventData : EventData
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
    /// 触摸更新事件类
    /// </summary>
    [System.Serializable]
    public class TouchUpdateEvent : ColliderEvent<TouchUpdateEventData>
    {
        public TouchUpdateEvent(Node node) : base(node)
        {
        }
    }
}
