using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Struct;

namespace EMR.Entity
{
    public class Pointer
    {
        #region 基本字段
        /// <summary>
        /// 目标对象
        /// </summary>
        public GameObject target;

        /// <summary>
        /// 命中点
        /// </summary>
        public Vector3? point;

        /// <summary>
        /// 命中点法线
        /// </summary>
        public Vector3? normal;

        public static bool hide = false;

        /// <summary>
        /// 惯用手
        /// </summary>
        public Handedness? handedness;

        private IMixedRealityPointer _ipointer;
        #endregion

        public Pointer(IMixedRealityPointer ipointer, GameObject target, Vector3? point, Vector3? normal, Handedness? handedness)
        {
            this._ipointer = ipointer;
            this.target = target;
            this.point = point;
            this.normal = normal;
            this.handedness = handedness;

            if(Pointer.hide)
            {
                if (this._ipointer is LinePointer)
                {
                    var renders = ((LinePointer)this._ipointer).LineRenderers;
                    foreach (var item in renders)
                    {
                        if (item.enabled)
                        {
                            item.enabled = false;
                        }
                    }
                }
            } else
            {
                if (this._ipointer is LinePointer)
                {
                    var renders = ((LinePointer)this._ipointer).LineRenderers;
                    foreach (var item in renders)
                    {
                        if(!item.enabled)
                        {
                            item.enabled = true;
                        }
                    }
                }
            }
        }
    }
}
