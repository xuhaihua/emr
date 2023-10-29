using UnityEngine;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// eye saccade EventData类
    /// </summary>
    public class EyeSaccadeEventData : EventData
    {
        /// <summary>
        /// 目标节点
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
        /// 距离
        /// </summary>
        public float distance;
    }

    /// <summary>
    /// eye saccade 事件类
    /// </summary>
    public class EyeSaccadeEvent : EMREvent<EyeSaccadeEventData>
    {
    }
}
