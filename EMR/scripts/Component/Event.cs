/*
 * 本文件主要用于定义组件的事件
 */

using EMR.Event;
using System.Collections.Generic;
using UnityEngine.Events;

namespace EMR
{
    public partial class Component
    {
        internal Dictionary<string, CustomEvent> customEvents = new Dictionary<string, CustomEvent>();

        /// <summary>
        /// 注册自定义事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="callback">事件处理方法</param>
        public void AddListener(string name, UnityAction<CustomEventData> callback)
        {
            if (!customEvents.ContainsKey(name))
            {
                customEvents.Add(name, new CustomEvent());
            }

            customEvents[name].AddListener(callback);
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="callback">事件处理方法</param>
        public void RemoveListener(string name, UnityAction<CustomEventData> callback)
        {
            if (customEvents.ContainsKey(name))
            {
                customEvents[name].RemoveListener(callback);
            }
        }

        /// <summary>
        /// 抛出事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="data">事件数据</param>
        public void emit(string name, CustomEventData eventData)
        {
            if (customEvents.ContainsKey(name))
            {
                eventData.target = this;

                customEvents[name].Invoke(eventData);
            }
        }

        protected readonly AppendEvent _onAppend = new AppendEvent();
        /// <summary>
        /// 添加组件前事件
        /// </summary>
        public AppendEvent onAppend
        {
            get
            {
                return this._onAppend;
            }
        }

        protected readonly AppendedEvent _onAppended = new AppendedEvent();
        /// <summary>
        /// 添加组件完成事件
        /// </summary>
        public AppendedEvent onAppended
        {
            get
            {
                return this._onAppended;
            }
        }

        protected readonly InsertEvent _onInsert = new InsertEvent();
        /// <summary>
        /// 插入组件前事件
        /// </summary>
        public virtual InsertEvent onInsert
        {
            get
            {
                return this._onInsert;
            }
        }

        protected readonly InsertedEvent _onInserted = new InsertedEvent();
        /// <summary>
        /// 插入组件完成事件
        /// </summary>
        public virtual InsertedEvent onInserted
        {
            get
            {
                return this._onInserted;
            }
        }

        protected readonly DestoryEvent _onDestory = new DestoryEvent();
        /// <summary>
        /// 销毁组件事件
        /// </summary>
        public DestoryEvent onDestory
        {
            get
            {
                return this._onDestory;
            }
        }

        protected readonly DestoryedEvent _onDestoryed = new DestoryedEvent();
        /// <summary>
        /// 销毁组件完成事件
        /// </summary>
        public DestoryedEvent onDestoryed
        {
            get
            {
                return this._onDestoryed;
            }
        }
    }
}