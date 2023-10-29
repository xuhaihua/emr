using UnityEngine;
using EMR.Entity;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// 释放EventData类
    /// </summary>
    public class UpEventData : EventData
    {
        /// <summary>
        /// 指针目标对象
        /// </summary>
        public Node target;

        /// <summary>
        /// 击种点的空间坐标
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

    /// <summary>
    /// 释放事件类
    /// </summary>
    [System.Serializable]
    public class UpEvent : ColliderEvent<UpEventData>
    {
        public UpEvent(Node node) : base(node)
        {
        }
    }
}