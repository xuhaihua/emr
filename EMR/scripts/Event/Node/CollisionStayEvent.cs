using UnityEngine;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 碰撞中EventData类
    /// </summary>
    public class CollisionStayEventData : EventData
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
    /// 碰撞中事件类
    /// </summary>
    [System.Serializable]
    public class CollisionStayEvent : ColliderEvent<CollisionStayEventData>
    {
        public CollisionStayEvent(Node node) : base(node)
        {
        }
    }
}


