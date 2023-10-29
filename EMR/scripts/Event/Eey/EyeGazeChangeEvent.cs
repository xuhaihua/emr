using UnityEngine;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// eye change EventData类
    /// </summary>
    public class EyeGazeChangeEventData : EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 原节点
        /// </summary>
        public Node oldNode;

        /// <summary>
        /// 击种点的空间坐标
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// 击种点的法线
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// 距离
        /// </summary>
        public float distance;
    }

    /// <summary>
    /// eye change 事件类
    /// </summary>
    public class EyeGazeChangeEvent : EMREvent<EyeGazeChangeEventData>
    {
    }
}
