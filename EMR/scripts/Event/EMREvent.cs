using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

namespace EMR.Event
{
    public class EMREvent<T> : UnityEvent<T> where T : EventData
    {
        private int _listenerCount = 0;

        private List<MethodInfo> eventHandlelist = new List<MethodInfo>();

        /// <summary>
        /// 侦听器数量
        /// </summary>
        public int listenerCount
        {
            get
            {
                return this._listenerCount;
            }
        }

        /// <summary>
        /// 注册事件侦听器
        /// </summary>
        /// <param name="handle">事件处理方法</param>
        public new void AddListener(UnityAction<T> handle)
        {
            if (eventHandlelist.IndexOf(handle.GetMethodInfo()) != -1)
            {
                return;
            }

            this._listenerCount++;
            base.AddListener(handle);
            eventHandlelist.Add(handle.GetMethodInfo());
        }

        /// <summary>
        /// 注销事件侦听器
        /// </summary>
        /// <param name="handle">事件处理方法</param>
        public new virtual void RemoveListener(UnityAction<T> handle)
        {
            if(eventHandlelist.IndexOf(handle.GetMethodInfo()) == -1)
            {
                return;
            }

            if (this._listenerCount > 0)
            {
                this._listenerCount--;
                base.RemoveListener(handle);
                eventHandlelist.Remove(handle.GetMethodInfo());
            }
        }
    }
}
