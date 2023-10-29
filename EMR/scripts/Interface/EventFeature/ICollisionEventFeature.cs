using EMR.Event;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发碰撞器碰撞事件能力
    /// </summary>
    public interface ICollisionEventFeature
    {
        /// <summary>
        /// 碰撞进入事件类
        /// </summary>
        CollisionEnterEvent onCollisionEnter { get; }

        /// <summary>
        /// 碰撞中事件类
        /// </summary>
        CollisionStayEvent onCollisionStay { get; }

        /// <summary>
        /// 碰撞退出事件类
        /// </summary>
        CollisionExitEvent onCollisionExit { get; }
    }
}

