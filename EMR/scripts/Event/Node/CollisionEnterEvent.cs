using UnityEngine;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 碰撞进入EventData类
    /// </summary>
    public class CollisionEnterEventData : EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 事件原始数据
        /// </summary>
        public Collision original;
    }

    /// <summary>
    /// 碰撞进入事件类
    /// </summary>
    [System.Serializable]
    public class CollisionEnterEvent : ColliderEvent<CollisionEnterEventData>
    {
        public CollisionEnterEvent(Node node) : base(node)
        {
        }
    }
}


