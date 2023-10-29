using EMR.Entity;
using EMR.Struct;

namespace EMR.Event
{
    /// <summary>
    /// 角度旋转量改变EventData类
    /// </summary>
    public class RotationChangeEventData : EventData
    {
        /// <summary>
        /// 目标节点
        /// </summary>
        public Node target;

        /// <summary>
        /// 旧旋转量
        /// </summary>
        public RotationData oldRotation;

        /// <summary>
        /// 当前旋转量
        /// </summary>
        public RotationData currentRotation;

        /// <summary>
        /// 阻止事件默认行为
        /// </summary>
        public override void preventDefault()
        {
            base.preventDefault();
            target.revertRotation();
        }
    }

    /// <summary>
    /// 角度旋转量改变事件类
    /// </summary>
    [System.Serializable]
    public class RotationChangeEvent : EMREvent<RotationChangeEventData>
    {
    }
}


