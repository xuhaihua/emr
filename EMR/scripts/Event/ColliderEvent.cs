using UnityEngine.Events;
using EMR.Entity;

namespace EMR.Event
{
    public class ColliderEvent<T> : EMREvent<T> where T : EventData
    {
        private Node node;

        public ColliderEvent(Node node)
        {
            this.node = node;
        }

        /// <summary>
        /// 注册事件侦听器
        /// </summary> 
        /// <param name="handle">事件处理方法</param>
        public new void AddListener(UnityAction<T> handle)
        {
            base.AddListener(handle);

            // 处理autoCollider
            if (!this.node.collider && this.node.autoCollider == null && (this.node.checkNodeIncludePointerEvent() || this.node.checkNodeIncludeTouchEvent() || this.node.checkNodeIncludeFocusEvent() || this.node.checkNodeIncludeColliderEvent()))
            {
                this.node.addAutoCollider();
            }

            // 处理autoInteractionTouchable
            if (this.node.interactionTouchableAuto && !this.node.hasAutoInteractionTouchable && this.node.checkNodeIncludeTouchEvent())
            {
                this.node.addAutoInteractionTouchable();
            }
        }

        /// <summary>
        /// 注销事件侦听器
        /// </summary>
        /// <param name="handle">事件处理方法</param>
        public new void RemoveListener(UnityAction<T> handle)
        {
            base.RemoveListener(handle);
        }
    }
}