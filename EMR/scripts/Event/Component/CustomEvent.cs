using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

namespace EMR.Event
{
    /// <summary>
    /// 客户自定义事件EventData类
    /// </summary>
    public class CustomEventData : EventData
    {
        /// <summary>
        /// 发起者
        /// </summary>
        public Component target;

        /// <summary>
        /// 事件携带的数据对象
        /// </summary>
        public object data;
    }

    /// <summary>
    /// 客户自定义事件类
    /// </summary>
    [System.Serializable]
    public class CustomEvent : UnityEvent<CustomEventData>
    {
        private List<MethodInfo> eventHandlelist = new List<MethodInfo>();

        /// <summary>
        /// 注册事件侦听器
        /// </summary>
        /// <param name="handle">事件处理方法</param>
        public new void AddListener(UnityAction<CustomEventData> handle)
        {
            if (eventHandlelist.IndexOf(handle.GetMethodInfo()) != -1)
            {
                return;
            }

            base.AddListener(handle);

            eventHandlelist.Add(handle.GetMethodInfo());
        }

        /// <summary>
        /// 注销事件侦听器
        /// </summary>
        /// <param name="handle">事件处理方法</param>
        public new virtual void RemoveListener(UnityAction<CustomEventData> handle)
        {
            if (eventHandlelist.IndexOf(handle.GetMethodInfo()) == -1)
            {
                return;
            }

            base.RemoveListener(handle);
            eventHandlelist.Remove(handle.GetMethodInfo());
        }
    }
}
