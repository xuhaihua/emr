using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EMR.Event
{
    /// <summary>
    /// 事件数据基类
    /// </summary>
    public class EventData
    {
        protected bool _isPreventDefault = false;

        public bool isPreventDefault
        {
            get
            {
                return this._isPreventDefault;
            }
        }

        public virtual void preventDefault()
        {
            this._isPreventDefault = true;
        }
    }
}
