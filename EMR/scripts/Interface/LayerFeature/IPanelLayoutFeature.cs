using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EMR.Struct;

namespace EMR
{
    /// <summary>
    /// 该节点具有平面布局能力
    /// </summary>
    public interface IPanelLayoutFeature
    {
        /// <summary>
        /// 内容对齐刷新
        /// </summary>
        void contentAlignFresh();

        /// <summary>
        /// 水平fudon刷新
        /// </summary>
        void horizontalFresh();

        /// <summary>
        /// 垂直浮动刷新
        /// </summary>
        void verticalFresh();
    }
}
