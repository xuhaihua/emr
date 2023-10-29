using UnityEngine;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 碰撞退出EventData类
    /// </summary>
    public class CollisionExitEventData : EventData
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
    /// 碰撞退出事件类
    /// </summary>
    [System.Serializable]
    public class CollisionExitEvent : ColliderEvent<CollisionExitEventData>
    {
        public CollisionExitEvent(Node node) : base(node)
        {
        }
    }
}


